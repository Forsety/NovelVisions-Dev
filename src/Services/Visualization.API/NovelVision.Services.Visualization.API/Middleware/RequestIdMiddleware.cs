// src/Services/Visualization.API/NovelVision.Services.Visualization.API/Middleware/RequestIdMiddleware.cs

namespace NovelVision.Services.Visualization.API.Middleware;

/// <summary>
/// Middleware for adding Request ID header
/// </summary>
public sealed class RequestIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string RequestIdHeader = "X-Request-Id";

    public RequestIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.Request.Headers[RequestIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");

        context.TraceIdentifier = requestId;
        context.Response.Headers[RequestIdHeader] = requestId;

        await _next(context);
    }
}