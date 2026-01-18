// src/Services/Visualization.API/NovelVision.Services.Visualization.Application/Behaviors/PerformanceBehavior.cs

using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace NovelVision.Services.Visualization.Application.Behaviors;

public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private const int WarningThresholdMs = 500;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        var response = await next();
        timer.Stop();

        if (timer.ElapsedMilliseconds > WarningThresholdMs)
        {
            _logger.LogWarning("Long running request: {RequestName} ({ElapsedMs}ms)",
                typeof(TRequest).Name, timer.ElapsedMilliseconds);
        }

        return response;
    }
}