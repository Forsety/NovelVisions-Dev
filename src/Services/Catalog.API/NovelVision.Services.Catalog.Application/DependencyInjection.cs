// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/DependencyInjection.cs
using System.Reflection;
using FluentValidation;
using MediatR;
using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;
using NovelVision.Services.Catalog.Application.Behaviors;
using NovelVision.Services.Catalog.Application.Services;

namespace NovelVision.Services.Catalog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.NotificationPublisher = new TaskWhenAllPublisher();
        });

        // AutoMapper - using the extension method from AutoMapper.Extensions.Microsoft.DependencyInjection
        services.AddAutoMapper(assembly);

        // FluentValidation - using the extension method from FluentValidation.DependencyInjectionExtensions
        services.AddValidatorsFromAssembly(assembly);

        // MediatR Pipeline Behaviors (order matters!)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        // Application Services
        services.AddScoped<IBookService, BookService>();
        // services.AddScoped<IVisualizationService, VisualizationService>(); // TODO: Implement

        return services;
    }
}