namespace NovelVision.Services.Catalog.API.Filters;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NovelVision.Services.Catalog.API.Models.Responses;
using System.Linq;

/// <summary>
/// Global validation filter
/// </summary>
public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var response = new ApiResponse
            {
                Success = false,
                Message = "Validation failed",
                Errors = errors.SelectMany(e => e.Value).ToList()
            };

            context.Result = new BadRequestObjectResult(response);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}

/// <summary>
/// Global exception filter
/// </summary>
public class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;
    private readonly IWebHostEnvironment _environment;

    public ApiExceptionFilter(
        ILogger<ApiExceptionFilter> logger,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception occurred");

        var response = new ApiResponse
        {
            Success = false,
            Message = _environment.IsDevelopment()
                ? context.Exception.Message
                : "An error occurred while processing your request",
            CorrelationId = context.HttpContext.Items["CorrelationId"]?.ToString()
        };

        if (_environment.IsDevelopment())
        {
            response.Errors = new List<string>
            {
                context.Exception.StackTrace ?? "No stack trace available"
            };
        }

        context.Result = new ObjectResult(response)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };

        context.ExceptionHandled = true;
    }
}
