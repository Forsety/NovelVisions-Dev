// src/Services/Catalog.API/NovelVision.Services.Catalog.API/Program.cs

using System.Reflection;
using System.Text.Json.Serialization;
using AspNetCoreRateLimit;
using HealthChecks.UI.Client;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using NovelVision.Services.Catalog.API.Filters;
using NovelVision.Services.Catalog.API.Middleware;
using NovelVision.Services.Catalog.Application;
using NovelVision.Services.Catalog.Infrastructure;
using NovelVision.Services.Catalog.Infrastructure.Persistence;
using System.IO.Compression;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/catalog-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting NovelVision Catalog API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .Enrich.WithProperty("ApplicationName", "NovelVision.Catalog.API"));

    // Add services to the container
    ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

    var app = builder.Build();

    // Configure the HTTP request pipeline
    await ConfigureAsync(app, app.Environment);

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
{
    // Add Application Insights (optional)
    if (!string.IsNullOrEmpty(configuration["ApplicationInsights:InstrumentationKey"]))
    {
        services.AddApplicationInsightsTelemetry(configuration);
    }

    // Add layers using extension methods from DependencyInjection
    services.AddApplication();
    services.AddInfrastructure(configuration);

    // Configure API Controllers
    services.AddControllers(options =>
    {
        // Global filters
        options.Filters.Add(new ProducesAttribute("application/json"));
        options.Filters.Add(new ConsumesAttribute("application/json"));

        // Model binding configuration
        options.MaxModelBindingCollectionSize = 100;
        options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
            _ => "The field is required.");
    })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var response = new
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = errors,
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = context.HttpContext.TraceIdentifier
                };

                return new BadRequestObjectResult(response);
            };
        });

    // API Versioning
    services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new HeaderApiVersionReader("api-version"),
            new QueryStringApiVersionReader("api-version"),
            new UrlSegmentApiVersionReader()
        );
    });

    services.AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // Swagger/OpenAPI Configuration
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "NovelVision Catalog API",
            Version = "v1",
            Description = "API for managing books, authors, and content visualization",
            Contact = new OpenApiContact
            {
                Name = "NovelVision Team",
                Email = "support@novelvision.com"
            }
        });

        // JWT Authentication
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        // Include XML comments
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }

        // Custom operation filters
        options.OperationFilter<SwaggerDefaultValuesFilter>();
        options.SchemaFilter<EnumSchemaFilter>();
    });

    // CORS Configuration
    services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins", policy =>
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:3000" };

            policy.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .SetPreflightMaxAge(TimeSpan.FromSeconds(86400))
                .WithExposedHeaders("Token-Expired", "X-Pagination", "X-Total-Count");
        });
    });

    // Response Caching
    services.AddResponseCaching(options =>
    {
        options.MaximumBodySize = 1024 * 1024 * 10; // 10MB
        options.SizeLimit = 100 * 1024 * 1024; // 100MB
    });

    // Response Compression
    services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<GzipCompressionProvider>();
        options.Providers.Add<BrotliCompressionProvider>();
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            new[] { "application/json", "text/json", "application/xml", "text/xml" });
    });

    services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Fastest;
    });

    services.Configure<BrotliCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Fastest;
    });

    // Rate Limiting Configuration
    services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
    services.Configure<ClientRateLimitOptions>(configuration.GetSection("ClientRateLimiting"));

    services.AddMemoryCache();
    services.AddInMemoryRateLimiting();
    services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
    services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
    services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

    // Health Checks
    var healthChecksBuilder = services.AddHealthChecks()
        .AddDbContextCheck<CatalogDbContext>("catalog-db",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "db", "sql", "catalog" })
        .AddCheck("self", () => HealthCheckResult.Healthy(),
            tags: new[] { "self" });

    // Add Redis health check only if configured
    var redisConnection = configuration.GetConnectionString("Redis");
    if (!string.IsNullOrEmpty(redisConnection))
    {
        healthChecksBuilder.AddRedis(redisConnection,
            name: "redis-cache",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "cache", "redis" });
    }

    services.AddHealthChecksUI(options =>
    {
        options.SetEvaluationTimeInSeconds(30);
        options.MaximumHistoryEntriesPerEndpoint(100);
        options.AddHealthCheckEndpoint("Catalog API", "/health");
    }).AddInMemoryStorage();

    // Hangfire Background Jobs (optional)
    if (configuration.GetValue<bool>("Hangfire:Enabled", false))
    {
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")
                ?? configuration.GetConnectionString("DefaultConnection"),
                new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

        services.AddHangfireServer(options =>
        {
            options.ServerName = $"{Environment.MachineName}:catalog-api";
            options.WorkerCount = Environment.ProcessorCount * 2;
            options.Queues = new[] { "critical", "default", "low" };
        });
    }

    // File Upload Configuration
    services.Configure<FormOptions>(options =>
    {
        options.ValueLengthLimit = int.MaxValue;
        options.MultipartBodyLengthLimit = 100_000_000; // 100MB
        options.MultipartBoundaryLengthLimit = 128;
    });

    // Add HttpClient for external services
    services.AddHttpClient("VisualizationService", client =>
    {
        client.BaseAddress = new Uri(configuration["ExternalServices:VisualizationApi"]
            ?? "https://localhost:7002/");
        client.Timeout = TimeSpan.FromSeconds(30);
    });

    // Forward headers for proxy scenarios
    services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    // Add custom middleware as services
    services.AddSingleton<ExceptionHandlingMiddleware>();
    services.AddTransient<RequestLoggingMiddleware>();
    services.AddTransient<CorrelationIdMiddleware>();

    // Register IWebHostEnvironment for ExceptionHandlingMiddleware
    services.AddSingleton<IWebHostEnvironment>(environment);
}

static async Task ConfigureAsync(WebApplication app, IWebHostEnvironment env)
{
    // Initialize database
    if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup", false))
    {
        using (var scope = app.Services.CreateScope())
        {
            try
            {
                await scope.ServiceProvider.InitializeDatabaseAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while migrating the database");
            }
        }
    }

    // Exception handling - MUST be first
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();

    // Development specific
    if (env.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog API V1");
            options.RoutePrefix = string.Empty;
            options.DocumentTitle = "NovelVision Catalog API Documentation";
            options.DisplayRequestDuration();
        });
    }
    else
    {
        app.UseHsts();
    }

    // Request pipeline
    app.UseHttpsRedirection();
    app.UseForwardedHeaders();
    app.UseResponseCompression();
    app.UseResponseCaching();

    // Request logging
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        };
    });

    // Rate limiting
    app.UseIpRateLimiting();

    // CORS
    app.UseCors("AllowSpecificOrigins");

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Health checks
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("db") || check.Tags.Contains("redis"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("self"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapHealthChecksUI(options => options.UIPath = "/health-ui");

    // Hangfire Dashboard (optional)
    if (app.Configuration.GetValue<bool>("Hangfire:Enabled", false))
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() },
            DashboardTitle = "NovelVision Jobs Dashboard"
        });
    }

    // Map controllers
    app.MapControllers();

    // Warmup endpoint
    app.MapGet("/", () => Results.Ok(new
    {
        Service = "NovelVision Catalog API",
        Version = "1.0.0",
        Status = "Running",
        Timestamp = DateTime.UtcNow
    })).AllowAnonymous();
}