// src/Services/Visualization.API/NovelVision.Services.Visualization.API/Filters/ApiExceptionFilterAttribute.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedKernelError = NovelVision.BuildingBlocks.SharedKernel.Results.Error;  // <-- ALIAS!

namespace NovelVision.Services.Visualization.API.Filters;

/// <summary>
/// Global exception filter for API
/// </summary>
public sealed class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    private readonly ILogger<ApiExceptionFilterAttribute> _logger;
    private readonly IHostEnvironment _environment;

    public ApiExceptionFilterAttribute(
        ILogger<ApiExceptionFilterAttribute> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public override void OnException(ExceptionContext context)
    {
        HandleException(context);
        base.OnException(context);
    }

    private void HandleException(ExceptionContext context)
    {
        var exception = context.Exception;

        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, error) = exception switch
        {
            ArgumentNullException => (StatusCodes.Status400BadRequest,
                SharedKernelError.Validation("A required argument was null")),

            ArgumentException argEx => (StatusCodes.Status400BadRequest,
                SharedKernelError.Validation(argEx.Message)),

            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized,
                SharedKernelError.Unauthorized("Unauthorized access")),

            InvalidOperationException invEx => (StatusCodes.Status400BadRequest,
                SharedKernelError.Validation(invEx.Message)),

            KeyNotFoundException => (StatusCodes.Status404NotFound,
                SharedKernelError.NotFound("Resource not found")),

            OperationCanceledException => (StatusCodes.Status499ClientClosedRequest,
                SharedKernelError.Failure("Operation was cancelled")),

            _ => (StatusCodes.Status500InternalServerError,
                SharedKernelError.Failure("An unexpected error occurred"))
        };

        var response = new ApiErrorResponse
        {
            Success = false,
            Error = new ApiError
            {
                Code = error.Code,
                Message = error.Message,
                Details = _environment.IsDevelopment() ? exception.ToString() : null
            },
            TraceId = context.HttpContext.TraceIdentifier
        };

        context.Result = new ObjectResult(response)
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;
    }
}

/// <summary>
/// API Error Response
/// </summary>
public sealed record ApiErrorResponse
{
    public bool Success { get; init; }
    public ApiError? Error { get; init; }
    public string? TraceId { get; init; }
}

/// <summary>
/// API Error Details
/// </summary>
public sealed record ApiError
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? Details { get; init; }
}