// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Behaviors/LoggingBehavior.cs
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace NovelVision.Services.Catalog.Application.Behaviors;

/// <summary>
/// Pipeline behavior для логирования запросов MediatR.
/// Совместим с MediatR 12+
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestGuid = Guid.NewGuid().ToString();

        _logger.LogInformation(
            "Handling {RequestName} [{RequestGuid}] {@Request}",
            requestName,
            requestGuid,
            request);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // MediatR 12+: просто await next()
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "Handled {RequestName} [{RequestGuid}] in {ElapsedMilliseconds}ms",
                requestName,
                requestGuid,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "Request {RequestName} [{RequestGuid}] failed after {ElapsedMilliseconds}ms",
                requestName,
                requestGuid,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}