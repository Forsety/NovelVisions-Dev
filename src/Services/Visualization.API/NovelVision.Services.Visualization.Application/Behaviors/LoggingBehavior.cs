using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace NovelVision.Services.Visualization.Application.Behaviors;

/// <summary>
/// Pipeline behavior для логирования
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
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
            "[START] {RequestName} [{RequestGuid}]",
            requestName, requestGuid);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "[END] {RequestName} [{RequestGuid}] completed in {ElapsedMs}ms",
                requestName, requestGuid, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "[ERROR] {RequestName} [{RequestGuid}] failed after {ElapsedMs}ms: {Message}",
                requestName, requestGuid, stopwatch.ElapsedMilliseconds, ex.Message);

            throw;
        }
    }
}
