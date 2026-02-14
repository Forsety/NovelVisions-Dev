// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Services/BackgroundJobs/ProcessVisualizationJobWorker.cs

using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.Services.Visualization.Application.Commands.ProcessJob;

namespace NovelVision.Services.Visualization.Infrastructure.Services.BackgroundJobs;

/// <summary>
/// Hangfire worker для обработки заданий визуализации
/// </summary>
public sealed class ProcessVisualizationJobWorker : IVisualizationJobProcessor
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProcessVisualizationJobWorker> _logger;

    public ProcessVisualizationJobWorker(
        IMediator mediator,
        ILogger<ProcessVisualizationJobWorker> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task ProcessAsync(Guid jobId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting to process visualization job {JobId}", jobId);

        try
        {
            var command = new ProcessJobCommand { JobId = jobId };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully processed visualization job {JobId}", jobId);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to process visualization job {JobId}: {Error}",
                    jobId, result.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while processing visualization job {JobId}", jobId);
            throw; // Hangfire will retry
        }
    }
}