// src/Services/Visualization.API/NovelVision.Services.Visualization.API/Program.cs

using System.IO.Compression;
using System.Text;
using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NovelVision.Services.Visualization.Application;
using NovelVision.Services.Visualization.Infrastructure;
using NovelVision.Services.Visualization.Infrastructure.Extensions;
using NovelVision.Services.Visualization.Infrastructure.Hubs;
using Serilog;

// Configure Serilog bootstrap logger
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/visualization-api-.txt", rollingInterval: Serilog.RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting NovelVision Visualization API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .Enrich.WithProperty("ApplicationName", "NovelVision.Visualization.API"));

    // Add services to the container
    ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

    var app = builder.Build();

    // Configure the HTTP request pipeline
    Configure(app, app.Environment);

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

static void ConfigureServices(
    IServiceCollection services,
    IConfiguration configuration,
    IWebHostEnvironment environment)
{
    // Add Application layer
    services.AddApplicationServices();

    // Add Infrastructure layer
    services.AddVisualizationInfrastructure(configuration);

    // Configure Controllers
    services.AddControllers(options =>
    {
        options.Filters.Add(new ProducesAttribute("application/json"));
        options.Filters.Add(new ConsumesAttribute("application/json"));
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    // CORS
    services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:3000", "https://localhost:3000" };

            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .WithExposedHeaders("X-Request-Id", "X-Total-Count");
        });
    });

    // Authentication
    ConfigureAuthentication(services, configuration);

    // Authorization
    services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireAuthenticatedUser", policy =>
            policy.RequireAuthenticatedUser());

        options.AddPolicy("RequireAdminRole", policy =>
            policy.RequireRole("Admin", "SuperAdmin"));

        options.AddPolicy("RequireAuthorRole", policy =>
            policy.RequireRole("Author", "Editor", "Admin", "SuperAdmin"));
    });

    // SignalR
    services.AddSignalR(options =>
    {
        options.EnableDetailedErrors = environment.IsDevelopment();
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    });

    // Response Compression
    services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            new[] { "application/json", "image/svg+xml" });
    });

    services.Configure<BrotliCompressionProviderOptions>(options =>
        options.Level = CompressionLevel.Fastest);

    services.Configure<GzipCompressionProviderOptions>(options =>
        options.Level = CompressionLevel.Fastest);

    // Memory Cache
    services.AddMemoryCache();

    // Health Checks
    services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(),
            tags: new[] { "self", "live" });

    // Swagger
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "NovelVision Visualization API",
            Version = "v1",
            Description = "AI-powered book visualization service for NovelVision platform",
            Contact = new OpenApiContact
            {
                Name = "NovelVision Team",
                Email = "support@novelvision.com"
            }
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
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
    });

    // HttpContextAccessor
    services.AddHttpContextAccessor();
}

static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
{
    var jwtSettings = configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"] ?? "NovelVision_Super_Secret_Key_For_JWT_Authentication_2024_Must_Be_At_Least_32_Characters";

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "NovelVision.Identity",
            ValidAudience = jwtSettings["Audience"] ?? "NovelVision.Services",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };

        // SignalR token handling
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/visualization"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });
}

static void Configure(WebApplication app, IWebHostEnvironment env)
{
    // Exception handling
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/error");
        app.UseHsts();
    }

    // HTTPS Redirection
    app.UseHttpsRedirection();

    // Response Compression
    app.UseResponseCompression();

    // CORS
    app.UseCors("AllowFrontend");

    // Swagger
    if (env.IsDevelopment() || env.IsStaging())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Visualization API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "NovelVision Visualization API";
            options.DisplayRequestDuration();
        });
    }

    // Static Files (для локального хранилища изображений)
    app.UseStaticFiles();

    // Routing
    app.UseRouting();

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Health Checks
    app.MapHealthChecks("/health");

    // Controllers
    app.MapControllers();

    // SignalR Hub
    app.MapHub<VisualizationHub>("/hubs/visualization");

    // Hangfire Dashboard (Development only)
    if (env.IsDevelopment())
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            DashboardTitle = "Visualization Jobs",
            Authorization = new[] { new HangfireAuthorizationFilter() }
        });
    }

    Log.Information("Visualization API started successfully");
    Log.Information($"Environment: {env.EnvironmentName}");
    Log.Information("Swagger UI: https://localhost:7130/swagger");
    Log.Information("Health Check: https://localhost:7130/health");
    Log.Information("SignalR Hub: wss://localhost:7130/hubs/visualization");
}

/// <summary>
/// Hangfire authorization filter (development only)
/// </summary>
public class HangfireAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        // В development разрешаем всем
        var httpContext = context.GetHttpContext();
        return httpContext.Request.Host.Host == "localhost";
    }
}