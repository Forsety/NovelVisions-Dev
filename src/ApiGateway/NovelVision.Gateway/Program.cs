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
using Swashbuckle.AspNetCore.SwaggerGen;

// Configure Serilog
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

    // Add Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.WithProperty("ServiceName", "NovelVision.Gateway")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName));

    // Load Ocelot configuration
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

static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
{
    // Add HttpContextAccessor first
    services.AddHttpContextAccessor();

    // Add CORS
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

    // Add Authentication
    ConfigureAuthentication(services, configuration);

    // Add Authorization
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

    // Add Rate Limiting
    ConfigureRateLimiting(services, configuration);

    // Add Health Checks
    services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(),
            tags: new[] { "gateway", "live" })
        .AddUrlGroup(new Uri("https://localhost:7295/health"), "catalog-api",
            Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
            tags: new[] { "catalog", "downstream" });

    // Add Controllers
    services.AddControllers();

    // Add API Documentation
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("gateway", new OpenApiInfo
        {
            Title = "NovelVision API Gateway",
            Version = "v1",
            Description = "API Gateway for NovelVision microservices architecture",
            Contact = new OpenApiContact
            {
                Name = "NovelVision Team",
                Email = "support@novelvision.com",
                Url = new Uri("https://novelvision.com")
            },
            License = new OpenApiLicense
            {
                Name = "MIT",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        });

        // Add JWT Authentication to Swagger
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = @"JWT Authorization header using the Bearer scheme.
                          Enter 'Bearer' [space] and then your token in the text input below.
                          Example: 'Bearer 12345abcdef'",
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
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });

        // Add custom filters (removed problematic filter)
        // options.OperationFilter<AddRequiredHeaderParameter>();

        // Include XML comments if available
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // Add Response Caching
    services.AddResponseCaching();

    // Add Ocelot with extensions
    services.AddOcelot(configuration)
        .AddCacheManager(settings =>
        {
            settings.WithDictionaryHandle()
                .WithExpiration(CacheManager.Core.ExpirationMode.Absolute, TimeSpan.FromMinutes(5));
        })
        .AddPolly();

    // Add custom services
    services.AddSingleton<IConfiguration>(configuration);
}

static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
{
    var jwtSettings = configuration.GetSection("JwtSettings");
    var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? "YourSecretKeyHere1234567890123456");

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
            ValidIssuer = jwtSettings["Issuer"] ?? "https://localhost:7000",
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"] ?? "https://localhost:7000",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RequireExpirationTime = true
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Information($"Token validated for user: {context.Principal?.Identity?.Name}");
                return Task.CompletedTask;
            }
        };
    });
}

static void ConfigureRateLimiting(IServiceCollection services, IConfiguration configuration)
{
    // Add memory cache for rate limiting
    services.AddMemoryCache();

    // Configure IP rate limiting
    services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
    services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));

    // Configure client rate limiting
    services.Configure<ClientRateLimitOptions>(configuration.GetSection("ClientRateLimiting"));
    services.Configure<ClientRateLimitPolicies>(configuration.GetSection("ClientRateLimitPolicies"));

    // Add rate limit stores
    services.AddInMemoryRateLimiting();
    services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
}

static async Task ConfigureAsync(WebApplication app, IWebHostEnvironment env, IConfiguration configuration)
{
    // Configure request pipeline order
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/error");
        app.UseHsts();
    }

    // Security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        await next();
    });

    // Request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.GetLevel = (httpContext, elapsed, ex) => ex != null
            ? LogEventLevel.Error
            : httpContext.Response.StatusCode > 499
                ? LogEventLevel.Error
                : LogEventLevel.Information;
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        };
    });

    // Basic middleware
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors("GatewayPolicy");

    // Rate limiting
    app.UseIpRateLimiting();
    app.UseClientRateLimiting();

    // Caching
    app.UseResponseCaching();

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Configure endpoints
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });
    });

    // Configure Health Check endpoints
    ConfigureHealthChecks(app);

    // Configure Swagger UI
    ConfigureSwagger(app, env);

    // Configure custom endpoints
    ConfigureCustomEndpoints(app);

    // Initialize Ocelot
    await app.UseOcelot();

    Log.Information("API Gateway started successfully");
    Log.Information($"Environment: {env.EnvironmentName}");
    Log.Information($"Base URL: {configuration["GlobalConfiguration:BaseUrl"]}");
    Log.Information("Swagger UI: https://localhost:7000/swagger");
    Log.Information("Health Check: https://localhost:7000/health");
}

static void ConfigureHealthChecks(WebApplication app)
{
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

static void ConfigureSwagger(WebApplication app, IWebHostEnvironment env)
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "swagger/{documentName}/swagger.json";
        options.SerializeAsV2 = false;
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/gateway/swagger.json", "Gateway API");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "NovelVision API Gateway";
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.ShowExtensions();
        options.ShowCommonExtensions();
        options.EnableValidator();

        if (env.IsDevelopment())
        {
            options.EnableTryItOutByDefault();
        }
    });
}

static void ConfigureCustomEndpoints(WebApplication app)
{
    // Root endpoint
    app.MapGet("/", () => Results.Ok(new
    {
        service = "NovelVision API Gateway",
        version = "1.0.0",
        status = "Running",
        timestamp = DateTime.UtcNow,
        documentation = "/swagger",
        health = "/health",
        routes = new[]
        {
            "/api/v1/catalog/books",
            "/api/v1/catalog/authors",
            "/api/v1/catalog/chapters",
            "/api/v1/catalog/pages"
        }
    })).AllowAnonymous()
       .WithName("GetGatewayInfo")
       .WithOpenApi();

    // Error endpoint
    app.Map("/error", appBuilder =>
    {
        appBuilder.Run(async context =>
        {
            var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            var exception = feature?.Error;

            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "An error occurred processing your request",
                message = exception?.Message,
                statusCode = 500,
                timestamp = DateTime.UtcNow,
                requestId = context.TraceIdentifier
            });
        });
    });
}