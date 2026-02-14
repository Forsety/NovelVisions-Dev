// src/ApiGateway/NovelVision.Gateway/Program.cs

using System.Text;
using AspNetCoreRateLimit;
using CacheManager.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using Serilog;
using Serilog.Events;

// ═══════════════════════════════════════════════════════════════════════════════
// Configure Serilog
// ═══════════════════════════════════════════════════════════════════════════════
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Ocelot", LogEventLevel.Debug)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting NovelVision API Gateway");

    var builder = WebApplication.CreateBuilder(args);

    // ═══════════════════════════════════════════════════════════════════════════
    // Configure Serilog Host
    // ═══════════════════════════════════════════════════════════════════════════
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.WithProperty("ServiceName", "NovelVision.Gateway")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName));

    // ═══════════════════════════════════════════════════════════════════════════
    // Load Ocelot configuration
    // ═══════════════════════════════════════════════════════════════════════════
    builder.Configuration
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
        .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();

    ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

    var app = builder.Build();

    await ConfigureAsync(app, app.Environment, app.Configuration);

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// ═══════════════════════════════════════════════════════════════════════════════
// Service Configuration
// ═══════════════════════════════════════════════════════════════════════════════
static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
{
    services.AddHttpContextAccessor();

    // ───────────────────────────────────────────────────────────────────────────
    // CORS Configuration
    // ───────────────────────────────────────────────────────────────────────────
    services.AddCors(options =>
    {
        options.AddPolicy("GatewayPolicy", policy =>
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:3000", "https://localhost:3000" };

            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .SetPreflightMaxAge(TimeSpan.FromSeconds(3600))
                  .WithExposedHeaders("Token-Expired", "X-Request-Id", "X-Total-Count");
        });
    });

    // ───────────────────────────────────────────────────────────────────────────
    // Authentication Configuration
    // ───────────────────────────────────────────────────────────────────────────
    ConfigureAuthentication(services, configuration);

    // ───────────────────────────────────────────────────────────────────────────
    // Authorization Policies
    // ───────────────────────────────────────────────────────────────────────────
    services.AddAuthorization(options =>
    {
        options.AddPolicy("ApiScope", policy =>
        {
            policy.RequireAuthenticatedUser();
        });

        options.AddPolicy("RequireAdminRole", policy =>
        {
            policy.RequireRole("Admin", "SuperAdmin");
        });

        options.AddPolicy("RequireAuthorRole", policy =>
        {
            policy.RequireRole("Author", "Editor", "Admin", "SuperAdmin");
        });
    });

    // ───────────────────────────────────────────────────────────────────────────
    // Rate Limiting
    // ───────────────────────────────────────────────────────────────────────────
    ConfigureRateLimiting(services, configuration);

    // ───────────────────────────────────────────────────────────────────────────
    // Health Checks (используем актуальные порты!)
    // ───────────────────────────────────────────────────────────────────────────
    services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
        .AddUrlGroup(new Uri("http://localhost:5231/health"), "catalog-api", HealthStatus.Degraded,
            tags: new[] { "ready" }, timeout: TimeSpan.FromSeconds(5))
        .AddUrlGroup(new Uri("http://localhost:8000/api/v1/health"), "promptgen-api", HealthStatus.Degraded,
            tags: new[] { "ready" }, timeout: TimeSpan.FromSeconds(5));

    // ───────────────────────────────────────────────────────────────────────────
    // Controllers & Swagger
    // ───────────────────────────────────────────────────────────────────────────
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("gateway", new OpenApiInfo
        {
            Title = "NovelVision API Gateway",
            Version = "v1",
            Description = "API Gateway for NovelVision microservices"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.",
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
                new List<string>()
            }
        });
    });

    // ───────────────────────────────────────────────────────────────────────────
    // Response Caching
    // ───────────────────────────────────────────────────────────────────────────
    services.AddResponseCaching();

    // ───────────────────────────────────────────────────────────────────────────
    // Ocelot with Extensions
    // ───────────────────────────────────────────────────────────────────────────
    services.AddOcelot(configuration)
        .AddCacheManager(settings =>
        {
            settings.WithDictionaryHandle()
                .WithExpiration(CacheManager.Core.ExpirationMode.Absolute, TimeSpan.FromMinutes(5));
        })
        .AddPolly();

    services.AddSingleton<IConfiguration>(configuration);
}

// ═══════════════════════════════════════════════════════════════════════════════
// Authentication Configuration
// ═══════════════════════════════════════════════════════════════════════════════
static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
{
    var jwtSettings = configuration.GetSection("JwtSettings");
    var key = Encoding.ASCII.GetBytes(
        jwtSettings["Secret"] ?? "YourSecretKeyHere1234567890123456");

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "NovelVision.Gateway",
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"] ?? "NovelVision.Client",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                Log.Warning("Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Debug("Token validated for user: {User}",
                    context.Principal?.Identity?.Name ?? "Unknown");
                return Task.CompletedTask;
            }
        };
    });
}

// ═══════════════════════════════════════════════════════════════════════════════
// Rate Limiting Configuration
// ═══════════════════════════════════════════════════════════════════════════════
static void ConfigureRateLimiting(IServiceCollection services, IConfiguration configuration)
{
    services.AddMemoryCache();
    services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
    services.Configure<ClientRateLimitOptions>(configuration.GetSection("ClientRateLimiting"));
    services.AddInMemoryRateLimiting();
    services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
}

// ═══════════════════════════════════════════════════════════════════════════════
// Application Pipeline Configuration
// ═══════════════════════════════════════════════════════════════════════════════
static async Task ConfigureAsync(WebApplication app, IWebHostEnvironment env, IConfiguration configuration)
{
    // Error handling
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/error");
        app.UseHsts();
    }

    // ╔═══════════════════════════════════════════════════════════════════════════╗
    // ║ CORS - MUST BE FIRST AFTER ERROR HANDLING!                               ║
    // ╚═══════════════════════════════════════════════════════════════════════════╝
    app.UseCors("GatewayPolicy");

    // Request pipeline
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress);
        };
    });

    // НЕ используем UseHttpsRedirection - ломает CORS preflight запросы!
    // app.UseHttpsRedirection();

    app.UseRouting();

    // Rate limiting
    app.UseIpRateLimiting();

    // Response caching
    app.UseResponseCaching();

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Map controllers
    app.MapControllers();

    // Health check endpoints
    ConfigureHealthChecks(app);

    // Swagger UI
    ConfigureSwagger(app, env);

    // Custom endpoints
    ConfigureCustomEndpoints(app);

    // Initialize Ocelot
    await app.UseOcelot();

    Log.Information("═══════════════════════════════════════════════════════════════");
    Log.Information("NovelVision API Gateway started successfully");
    Log.Information("═══════════════════════════════════════════════════════════════");
    Log.Information($"Environment: {env.EnvironmentName}");
    Log.Information($"Base URL: http://localhost:5000");
    Log.Information("───────────────────────────────────────────────────────────────");
    Log.Information("Available Endpoints:");
    Log.Information("  Swagger UI:         http://localhost:5000/swagger");
    Log.Information("  Health Check:       http://localhost:5000/health");
    Log.Information("───────────────────────────────────────────────────────────────");
    Log.Information("Downstream Services:");
    Log.Information("  Catalog API:        http://localhost:5231 → /catalog/*, /auth/*");
    Log.Information("  PromptGen API:      http://localhost:8000 → /promptgen/*");
    Log.Information("═══════════════════════════════════════════════════════════════");
}

// ═══════════════════════════════════════════════════════════════════════════════
// Health Checks Configuration
// ═══════════════════════════════════════════════════════════════════════════════
static void ConfigureHealthChecks(WebApplication app)
{
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live"),
        ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
    });
}

// ═══════════════════════════════════════════════════════════════════════════════
// Swagger Configuration
// ═══════════════════════════════════════════════════════════════════════════════
static void ConfigureSwagger(WebApplication app, IWebHostEnvironment env)
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "swagger/{documentName}/swagger.json";
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/gateway/swagger.json", "Gateway API");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "NovelVision API Gateway";
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.EnableFilter();
    });
}

// ═══════════════════════════════════════════════════════════════════════════════
// Custom Endpoints
// ═══════════════════════════════════════════════════════════════════════════════
static void ConfigureCustomEndpoints(WebApplication app)
{
    app.MapGet("/", () => Results.Ok(new
    {
        service = "NovelVision API Gateway",
        version = "1.0.0",
        status = "running",
        timestamp = DateTime.UtcNow,
        documentation = "/swagger",
        health = "/health"
    }));

    app.MapGet("/error", () => Results.Problem(
        title: "An error occurred",
        statusCode: StatusCodes.Status500InternalServerError));

    app.MapGet("/services", () => Results.Ok(new
    {
        gateway = new { name = "NovelVision.Gateway", status = "healthy", url = "http://localhost:5000" },
        downstream = new[]
        {
            new { name = "Catalog.API", prefix = "/catalog, /auth", url = "http://localhost:5231" },
            new { name = "PromptGen.API", prefix = "/promptgen", url = "http://localhost:8000" }
        }
    }));
}