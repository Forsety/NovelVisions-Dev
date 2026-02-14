using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NovelVision.Services.Visualization.Application.Behaviors;
using System.Reflection;

namespace NovelVision.Services.Visualization.Application.Extensions;

/// <summary>
/// Расширения для регистрации зависимостей Application слоя
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Добавить сервисы Application слоя
    /// </summary>
    public static IServiceCollection AddVisualizationApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // AutoMapper
        

        return services;
    }
}
