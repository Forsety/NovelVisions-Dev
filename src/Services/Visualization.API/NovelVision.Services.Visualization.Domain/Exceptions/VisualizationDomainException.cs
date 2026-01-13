using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Domain.Exceptions;

/// <summary>
/// Базовое исключение домена визуализации
/// </summary>
public class VisualizationDomainException : Exception
{
    public VisualizationDomainException(string message) : base(message)
    {
    }

    public VisualizationDomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Задание визуализации не найдено
/// </summary>
public class VisualizationJobNotFoundException : VisualizationDomainException
{
    public VisualizationJobNotFoundException(VisualizationJobId jobId)
        : base($"Visualization job with ID {jobId.Value} was not found")
    {
        JobId = jobId;
    }

    public VisualizationJobId JobId { get; }
}

/// <summary>
/// Изображение не найдено
/// </summary>
public class GeneratedImageNotFoundException : VisualizationDomainException
{
    public GeneratedImageNotFoundException(GeneratedImageId imageId)
        : base($"Generated image with ID {imageId.Value} was not found")
    {
        ImageId = imageId;
    }

    public GeneratedImageId ImageId { get; }
}

/// <summary>
/// Недопустимый переход статуса
/// </summary>
public class InvalidJobStatusTransitionException : VisualizationDomainException
{
    public InvalidJobStatusTransitionException(string fromStatus, string toStatus)
        : base($"Cannot transition from status '{fromStatus}' to '{toStatus}'")
    {
        FromStatus = fromStatus;
        ToStatus = toStatus;
    }

    public string FromStatus { get; }
    public string ToStatus { get; }
}

/// <summary>
/// Превышен лимит повторных попыток
/// </summary>
public class MaxRetriesExceededException : VisualizationDomainException
{
    public MaxRetriesExceededException(VisualizationJobId jobId, int maxRetries)
        : base($"Job {jobId.Value} has exceeded maximum retry count of {maxRetries}")
    {
        JobId = jobId;
        MaxRetries = maxRetries;
    }

    public VisualizationJobId JobId { get; }
    public int MaxRetries { get; }
}

/// <summary>
/// Ошибка AI провайдера
/// </summary>
public class AIProviderException : VisualizationDomainException
{
    public AIProviderException(string provider, string message, string? errorCode = null)
        : base($"AI provider '{provider}' error: {message}")
    {
        Provider = provider;
        ErrorCode = errorCode;
    }

    public string Provider { get; }
    public string? ErrorCode { get; }
}

/// <summary>
/// Ошибка генерации промпта
/// </summary>
public class PromptGenerationException : VisualizationDomainException
{
    public PromptGenerationException(string message)
        : base($"Prompt generation failed: {message}")
    {
    }

    public PromptGenerationException(string message, Exception innerException)
        : base($"Prompt generation failed: {message}", innerException)
    {
    }
}

/// <summary>
/// Книга не поддерживает визуализацию
/// </summary>
public class VisualizationNotSupportedException : VisualizationDomainException
{
    public VisualizationNotSupportedException(Guid bookId, string reason)
        : base($"Visualization is not supported for book {bookId}: {reason}")
    {
        BookId = bookId;
        Reason = reason;
    }

    public Guid BookId { get; }
    public string Reason { get; }
}
