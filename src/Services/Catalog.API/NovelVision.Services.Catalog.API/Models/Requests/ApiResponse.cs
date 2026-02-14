namespace NovelVision.Services.Catalog.API.Models.Responses;

using System;
using System.Collections.Generic;

/// <summary>
/// Base API response model
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
}

/// <summary>
/// Generic API response with data
/// </summary>
public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }
}
