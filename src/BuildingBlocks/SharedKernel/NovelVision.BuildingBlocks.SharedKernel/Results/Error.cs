// src/BuildingBlocks/SharedKernel/NovelVision.BuildingBlocks.SharedKernel/Results/Error.cs
namespace NovelVision.BuildingBlocks.SharedKernel.Results;

/// <summary>
/// Represents an error in the system
/// </summary>
public class Error
{
    /// <summary>
    /// Gets the error message
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the error code (optional)
    /// </summary>
    public string? Code { get; }

    /// <summary>
    /// Gets the error type
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets additional metadata about the error
    /// </summary>
    public Dictionary<string, object>? Metadata { get; }

    /// <summary>
    /// Private constructor to enforce factory methods
    /// </summary>
    private Error(string message, string? code = null, string type = "Failure", Dictionary<string, object>? metadata = null)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Code = code;
        Type = type;
        Metadata = metadata ?? new Dictionary<string, object> { ["Type"] = type };

        // Ensure Type is always in metadata
        if (!Metadata.ContainsKey("Type"))
        {
            Metadata["Type"] = type;
        }
    }

    #region Factory Methods

    /// <summary>
    /// Creates a validation error
    /// </summary>
    public static Error Validation(string message, string? code = "ValidationError")
    {
        return new Error(message, code, ErrorType.Validation);
    }

    /// <summary>
    /// Creates a not found error
    /// </summary>
    public static Error NotFound(string message, string? code = "NotFound")
    {
        return new Error(message, code, ErrorType.NotFound);
    }

    /// <summary>
    /// Creates a conflict error
    /// </summary>
    public static Error Conflict(string message, string? code = "Conflict")
    {
        return new Error(message, code, ErrorType.Conflict);
    }

    /// <summary>
    /// Creates an unauthorized error
    /// </summary>
    public static Error Unauthorized(string message = "Unauthorized access", string? code = "Unauthorized")
    {
        return new Error(message, code, ErrorType.Unauthorized);
    }

    /// <summary>
    /// Creates a forbidden error
    /// </summary>
    public static Error Forbidden(string message = "Access forbidden", string? code = "Forbidden")
    {
        return new Error(message, code, ErrorType.Forbidden);
    }

    /// <summary>
    /// Creates a failure error
    /// </summary>
    public static Error Failure(string message, string? code = "GeneralFailure")
    {
        return new Error(message, code, ErrorType.Failure);
    }

    /// <summary>
    /// Creates an unexpected/internal error
    /// </summary>
    public static Error Unexpected(string message, string? code = "UnexpectedError")
    {
        return new Error(message, code, "Unexpected", new Dictionary<string, object> { ["Type"] = "Unexpected" });
    }

    /// <summary>
    /// Creates a custom error
    /// </summary>
    public static Error Custom(string message, string code, string type, Dictionary<string, object>? metadata = null)
    {
        var meta = metadata ?? new Dictionary<string, object>();
        meta["Type"] = type;
        return new Error(message, code, type, meta);
    }

    #endregion

    /// <summary>
    /// Implicit conversion from string to Error
    /// </summary>
    public static implicit operator Error(string message)
    {
        return Failure(message);
    }

    /// <summary>
    /// String representation of the error
    /// </summary>
    public override string ToString()
    {
        return Code != null ? $"[{Code}] {Message}" : Message;
    }

    /// <summary>
    /// Checks if this is a specific type of error
    /// </summary>
    public bool IsType(string errorType)
    {
        return Type == errorType ||
               (Metadata?.TryGetValue("Type", out var type) == true && type?.ToString() == errorType);
    }
}