using System.Net;
using System.Text.Json;
using NovelVision.Services.Catalog.API.Models.Requests;
using NovelVision.Services.Catalog.API.Models.Responses;
using NovelVision.Services.Catalog.Application.Exceptions;

namespace NovelVision.Services.Catalog.API.Middleware;

public class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Success = false,
            Timestamp = DateTime.UtcNow,
            CorrelationId = context.TraceIdentifier,
            Instance = context.Request.Path
        };

        switch (exception)
        {
            case Application.Exceptions.ValidationException validationException:
                response.Status = (int)HttpStatusCode.BadRequest;
                response.Type = "ValidationError";
                response.Message = "Validation failed";
                response.Detail = validationException.Message;
                response.ValidationErrors = (Dictionary<string, List<string>>?)validationException.Errors;
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case Application.Exceptions.NotFoundException notFoundException:
                response.Status = (int)HttpStatusCode.NotFound;
                response.Type = "NotFound";
                response.Message = "Resource not found";
                response.Detail = notFoundException.Message;
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;

            case Application.Exceptions.UnauthorizedException unauthorizedException:
                response.Status = (int)HttpStatusCode.Unauthorized;
                response.Type = "Unauthorized";
                response.Message = "Unauthorized";
                response.Detail = unauthorizedException.Message;
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;

            case Application.Exceptions.ForbiddenException forbiddenException:
                response.Status = (int)HttpStatusCode.Forbidden;
                response.Type = "Forbidden";
                response.Message = "Access denied";
                response.Detail = forbiddenException.Message;
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                break;

            case Application.Exceptions.ConflictException conflictException:
                response.Status = (int)HttpStatusCode.Conflict;
                response.Type = "Conflict";
                response.Message = "Conflict";
                response.Detail = conflictException.Message;
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                break;

            case Application.Exceptions.BusinessRuleException businessRuleException:
                response.Status = (int)HttpStatusCode.UnprocessableEntity;
                response.Type = "BusinessRuleViolation";
                response.Message = "Business rule violation";
                response.Detail = businessRuleException.Message;
                response.Errors = new List<string> { businessRuleException.Rule };
                context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                break;

            case TimeoutException timeoutException:
                response.Status = (int)HttpStatusCode.RequestTimeout;
                response.Type = "Timeout";
                response.Message = "Request timeout";
                response.Detail = timeoutException.Message;
                context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                break;

            case OperationCanceledException:
                response.Status = 499; // Client Closed Request
                response.Type = "Cancelled";
                response.Message = "Request was cancelled";
                response.Detail = "The request was cancelled by the client";
                context.Response.StatusCode = 499;
                break;

            default:
                response.Status = (int)HttpStatusCode.InternalServerError;
                response.Type = "InternalServerError";
                response.Message = "An error occurred while processing your request";

                // Only show detailed error in development
                if (_environment.IsDevelopment())
                {
                    response.Detail = exception.ToString();
                }
                else
                {
                    response.Detail = "An internal server error occurred";
                }

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
