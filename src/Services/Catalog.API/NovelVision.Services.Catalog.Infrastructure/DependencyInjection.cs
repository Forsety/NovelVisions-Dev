// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/DependencyInjection.cs
using System;
using System.Text;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NovelVision.BuildingBlocks.SharedKernel.Repositories;
using NovelVision.Services.Catalog.Application.Common.Interfaces;
using NovelVision.Services.Catalog.Application.Interfaces;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.Services;
using NovelVision.Services.Catalog.Infrastructure.Identity.Entities;
using NovelVision.Services.Catalog.Infrastructure.Identity.Persistence;
using NovelVision.Services.Catalog.Infrastructure.Identity.Services;
using NovelVision.Services.Catalog.Infrastructure.Identity.Settings;
using NovelVision.Services.Catalog.Infrastructure.Persistence;
using NovelVision.Services.Catalog.Infrastructure.Persistence.Interceptors;
using NovelVision.Services.Catalog.Infrastructure.Persistence.Repositories;
using NovelVision.Services.Catalog.Infrastructure.Services;
using NovelVision.Services.Catalog.Infrastructure.Services.Cache;
using NovelVision.Services.Catalog.Infrastructure.Services.DateTime;
using NovelVision.Services.Catalog.Infrastructure.Services.Email;
using NovelVision.Services.Catalog.Infrastructure.Services.External;
using NovelVision.Services.Catalog.Infrastructure.Services.Import;
using NovelVision.Services.Catalog.Infrastructure.Services.Storage;
using SendGrid;
using SendGrid.Extensions.DependencyInjection;

namespace NovelVision.Services.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add HttpContextAccessor for CurrentUserService
        services.AddHttpContextAccessor();

        // Register Services needed by Interceptors
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTime, DateTimeService>();

        // Register Interceptors as Scoped
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        // ============================================
        // DATABASE CONTEXTS
        // ============================================

        // Configure Main Database Context
        services.AddDbContext<CatalogDbContext>((serviceProvider, options) =>
        {
            var auditableInterceptor = serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>();
            var domainEventsInterceptor = serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>();

            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName))
                .AddInterceptors(auditableInterceptor, domainEventsInterceptor);

            if (configuration.GetValue<bool>("EnableDetailedLogging", false))
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
                options.LogTo(Console.WriteLine, LogLevel.Information);
            }
        });

        // Configure Identity Database Context
        services.AddDbContext<ApplicationIdentityDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("IdentityConnection")
                    ?? configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationIdentityDbContext).Assembly.FullName));
        });

        // ============================================
        // IDENTITY CONFIGURATION
        // ============================================

        // Configure JWT Settings
        var jwtSettingsSection = configuration.GetSection("JwtSettings");
        services.Configure<JwtSettings>(jwtSettingsSection);
        var jwtSettings = jwtSettingsSection.Get<JwtSettings>() ?? new JwtSettings();

        // Configure Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 4;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

            // SignIn settings
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
        .AddDefaultTokenProviders();

        // Configure JWT Authentication
        var key = Encoding.ASCII.GetBytes(jwtSettings.Secret ?? "DefaultSecretKeyForDevelopmentOnly123456789!");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = configuration.GetValue<bool>("Identity:RequireHttps", false);
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = jwtSettings.ValidateIssuer,
                ValidateAudience = jwtSettings.ValidateAudience,
                ValidateLifetime = jwtSettings.ValidateLifetime,
                ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkewMinutes)
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
                    {
                        context.Fail("Unauthorized");
                    }
                    return System.Threading.Tasks.Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Append("Token-Expired", "true");
                    }
                    return System.Threading.Tasks.Task.CompletedTask;
                }
            };
        });

        // Register Identity Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IdentityDataSeeder>();

        // Configure Authorization Policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdminRole",
                policy => policy.RequireRole(ApplicationRoles.Admin, ApplicationRoles.SuperAdmin));

            options.AddPolicy("RequireAuthorRole",
                policy => policy.RequireRole(ApplicationRoles.Author, ApplicationRoles.Editor, ApplicationRoles.Admin, ApplicationRoles.SuperAdmin));

            options.AddPolicy("RequireModeratorRole",
                policy => policy.RequireRole(ApplicationRoles.Moderator, ApplicationRoles.Admin, ApplicationRoles.SuperAdmin));

            options.AddPolicy("CanManageBooks",
                policy => policy.RequireClaim("permission", "books.manage", "books.create"));

            options.AddPolicy("CanManageUsers",
                policy => policy.RequireClaim("permission", "users.manage"));
        });

        // ============================================
        // UNIT OF WORK & REPOSITORIES
        // ============================================

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<CatalogDbContext>());
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<IPageRepository, PageRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();

        // ============================================
        // DOMAIN SERVICES
        // ============================================

        services.AddScoped<IBookDomainService, BookDomainService>();
        services.AddScoped<IVisualizationSettingsService, VisualizationSettingsService>();

        // ============================================
        // GUTENBERG IMPORT SERVICES (ИСПРАВЛЕНИЕ!)
        // ============================================

        // HttpClient для Gutendex API
        services.AddHttpClient<IGutendexService, GutendexService>(client =>
        {
            client.BaseAddress = new Uri("https://gutendex.com");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "NovelVision/1.0");
        });

        // Text parser для разбора книг Gutenberg
        services.AddScoped<GutenbergTextParser>();

        // Text parsing service для парсинга текста
        services.AddScoped<ITextParsingService, TextParsingService>();

        // Book import service
        services.AddScoped<IBookImportService, BookImportService>();

        // ============================================
        // CACHING
        // ============================================

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "NovelVision:";
            });
            services.AddScoped<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddDistributedMemoryCache();
            services.AddScoped<ICacheService, RedisCacheService>();
        }

        // ============================================
        // EMAIL SERVICE
        // ============================================

        var sendGridApiKey = configuration["SendGrid:ApiKey"];
        if (!string.IsNullOrEmpty(sendGridApiKey))
        {
            services.AddSendGrid(options =>
            {
                options.ApiKey = sendGridApiKey;
            });
            services.AddScoped<IEmailService, EmailService>();
        }
        else
        {
            services.AddScoped<IEmailService, MockEmailService>();
        }

        // ============================================
        // STORAGE SERVICE
        // ============================================

        var azureStorageConnection = configuration.GetConnectionString("AzureStorage");
        if (!string.IsNullOrEmpty(azureStorageConnection))
        {
            services.AddSingleton(x => new BlobServiceClient(azureStorageConnection));
            services.AddScoped<IFileStorageService, AzureBlobStorageService>();
        }
        else
        {
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
        }

        return services;
    }

    /// <summary>
    /// Ensures database is created and migrations are applied
    /// </summary>
    public static async System.Threading.Tasks.Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var catalogContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var identityContext = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentityDataSeeder>();

        // Apply migrations
        await catalogContext.Database.MigrateAsync();
        await identityContext.Database.MigrateAsync();

        // Seed initial data
        await seeder.SeedAsync();
    }
}