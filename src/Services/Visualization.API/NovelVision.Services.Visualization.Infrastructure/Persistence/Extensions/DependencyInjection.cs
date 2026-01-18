// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Extensions/DependencyInjection.cs

using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.Repositories;
using NovelVision.Services.Visualization.Infrastructure.Persistence;
using NovelVision.Services.Visualization.Infrastructure.Persistence.Interceptors;
using NovelVision.Services.Visualization.Infrastructure.Persistence.Repositories;
using NovelVision.Services.Visualization.Infrastructure.Services.AIProviders;
using NovelVision.Services.Visualization.Infrastructure.Services.BackgroundJobs;
using NovelVision.Services.Visualization.Infrastructure.Services.Cache;
using NovelVision.Services.Visualization.Infrastructure.Services.Common;
using NovelVision.Services.Visualization.Infrastructure.Services.External;
using NovelVision.Services.Visualization.Infrastructure.Services.Notifications;
using NovelVision.Services.Visualization.Infrastructure.Services.Queue;
using NovelVision.Services.Visualization.Infrastructure.Services.Storage;
using NovelVision.Services.Visualization.Infrastructure.Settings;
using Polly;
using Polly.Extensions.Http;
using StackExchange.Redis;

namespace NovelVision.Services.Visualization.Infrastructure.Extensions;

/// <summary>
/// Расширения для регистрации зависимостей Infrastructure слоя
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Добавить сервисы Infrastructure слоя
    /// </summary>
    public static IServiceCollection AddVisualizationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ══════════════════════════════════════════════════════════════
        // SETTINGS
        // ══════════════════════════════════════════════════════════════
        services.Configure<AIProviderSettings>(configuration.GetSection("AIProviders"));
        services.Configure<AzureStorageSettings>(configuration.GetSection("AzureStorage"));
        services.Configure<RedisSettings>(configuration.GetSection("Redis"));
        services.Configure<ExternalServicesSettings>(configuration.GetSection("ExternalServices"));

        // ══════════════════════════════════════════════════════════════
        // COMMON SERVICES
        // ══════════════════════════════════════════════════════════════
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // ══════════════════════════════════════════════════════════════
        // INTERCEPTORS
        // ══════════════════════════════════════════════════════════════
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        // ══════════════════════════════════════════════════════════════
        // DATABASE
        // ══════════════════════════════════════════════════════════════
        var connectionString = configuration.GetConnectionString("VisualizationDb")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not configured");

        services.AddDbContext<VisualizationDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(VisualizationDbContext).Assembly.FullName);
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "visualization");
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(60);
            });

            // Add interceptors
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<DispatchDomainEventsInterceptor>());

            // Enable sensitive data logging in development
#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        // ══════════════════════════════════════════════════════════════
        // REPOSITORIES
        // ══════════════════════════════════════════════════════════════
        services.AddScoped<IVisualizationJobRepository, VisualizationJobRepository>();
        services.AddScoped<IGeneratedImageRepository, GeneratedImageRepository>();

        // ══════════════════════════════════════════════════════════════
        // REDIS CACHE
        // ══════════════════════════════════════════════════════════════
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var config = ConfigurationOptions.Parse(redisConnection);
                config.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(config);
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "NovelVision.Visualization:";
            });

            services.AddScoped<IVisualizationCacheService, RedisCacheService>();
            services.AddScoped<IJobQueueService, RedisJobQueueService>();
        }
        else
        {
            // In-memory fallback for development
            services.AddDistributedMemoryCache();
            services.AddScoped<IVisualizationCacheService, InMemoryCacheService>();
            services.AddScoped<IJobQueueService, InMemoryJobQueueService>();
        }

        // ══════════════════════════════════════════════════════════════
        // HTTP CLIENTS (External Services)
        // ══════════════════════════════════════════════════════════════
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

        // Catalog.API client
        services.AddHttpClient<ICatalogService, CatalogService>((sp, client) =>
        {
            var settings = configuration.GetSection("ExternalServices").Get<ExternalServicesSettings>();
            client.BaseAddress = new Uri(settings?.CatalogApiUrl ?? "https://localhost:7295");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);

        // PromptGen.API client
        services.AddHttpClient<IPromptGenService, PromptGenService>((sp, client) =>
        {
            var settings = configuration.GetSection("ExternalServices").Get<ExternalServicesSettings>();
            client.BaseAddress = new Uri(settings?.PromptGenApiUrl ?? "http://localhost:8000");
            client.Timeout = TimeSpan.FromSeconds(60);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);

        // ══════════════════════════════════════════════════════════════
        // AI PROVIDERS
        // ══════════════════════════════════════════════════════════════
        services.AddScoped<DallE3Service>();
        services.AddScoped<StableDiffusionService>();
        services.AddScoped<IAIImageGeneratorService, AIImageGeneratorFactory>();

        // ══════════════════════════════════════════════════════════════
        // STORAGE
        // ══════════════════════════════════════════════════════════════
        var azureStorageConnection = configuration.GetConnectionString("AzureStorage");
        if (!string.IsNullOrEmpty(azureStorageConnection))
        {
            services.AddScoped<IImageStorageService, AzureBlobStorageService>();
        }
        else
        {
            services.AddScoped<IImageStorageService, LocalStorageService>();
        }

        // ══════════════════════════════════════════════════════════════
        // SIGNALR NOTIFICATIONS
        // ══════════════════════════════════════════════════════════════
        services.AddScoped<IVisualizationNotificationService, SignalRNotificationService>();

        // ══════════════════════════════════════════════════════════════
        // BACKGROUND JOBS (Hangfire)
        // ══════════════════════════════════════════════════════════════
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                SchemaName = "hangfire_visualization",
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.FromSeconds(15),
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 2;
            options.Queues = new[] { "visualization", "default" };
        });

        services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

        return services;
    }
}