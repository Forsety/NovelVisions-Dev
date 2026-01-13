using System.Diagnostics;
using System.IO;
using System.Text;

namespace NovelVision.Services.Catalog.API.Middleware;

public class RequestLoggingMiddleware : IMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private static readonly List<string> SensitiveHeaders = new()
    {
        "Authorization",
        "Cookie",
        "Set-Cookie"
    };

    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            // Log request
            await LogRequestAsync(context);

            // Capture response
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await next(context);

            // Log response
            await LogResponseAsync(context, responseBody, stopwatch.ElapsedMilliseconds);

            // Copy response to original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogRequestAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        var requestBody = string.Empty;
        if (context.Request.ContentLength > 0 && context.Request.ContentLength < 10240) // 10KB limit
        {
            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        var headers = context.Request.Headers
            .Where(h => !SensitiveHeaders.Contains(h.Key, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(h => h.Key, h => h.Value.ToString());

        _logger.LogInformation("HTTP Request Information: " +
            "Method: {Method}, " +
            "Path: {Path}, " +
            "QueryString: {QueryString}, " +
            "Headers: {@Headers}, " +
            "Body: {Body}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString.Value,
            headers,
            requestBody);
    }

    private async Task LogResponseAsync(HttpContext context, MemoryStream responseBody, long elapsedMs)
    {
        responseBody.Seek(0, SeekOrigin.Begin);
        var responseText = string.Empty;

        if (responseBody.Length < 10240) // 10KB limit
        {
            responseText = await new StreamReader(responseBody).ReadToEndAsync();
        }

        responseBody.Seek(0, SeekOrigin.Begin);

        _logger.LogInformation("HTTP Response Information: " +
            "StatusCode: {StatusCode}, " +
            "ElapsedMs: {ElapsedMs}, " +
            "Body: {Body}",
            context.Response.StatusCode,
            elapsedMs,
            responseText);

        // Log warning for slow requests
        if (elapsedMs > 1000)
        {
            _logger.LogWarning("Slow request detected: {Method} {Path} took {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                elapsedMs);
        }
    }
}
