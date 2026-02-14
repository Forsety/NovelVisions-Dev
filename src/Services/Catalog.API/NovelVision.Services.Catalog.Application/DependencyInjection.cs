// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/DependencyInjection.cs
using System.Reflection;
using AutoMapper;
using FluentValidation;
using MediatR;
using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;
using NovelVision.Services.Catalog.Application.Behaviors;
using NovelVision.Services.Catalog.Application.Services;
using NovelVision.Services.Visualization.Application.Behaviors;

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

        // AutoMapper 13+ - передаем тип из Assembly
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(DependencyInjection).Assembly));
        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // MediatR Pipeline Behaviors (order matters!)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        // Application Services
        services.AddScoped<IBookService, BookService>();

        return services;
    }
}