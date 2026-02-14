// src/Services/Visualization.API/NovelVision.Services.Visualization.API/Models/Responses/ApiResponse.cs

namespace NovelVision.Services.Visualization.API.Models.Responses;

/// <summary>
/// Standard API response wrapper
/// </summary>
public record ApiResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? TraceId { get; init; }

    public static ApiResponse Ok(string? message = null) =>
        new() { Success = true, Message = message };

    public static ApiResponse Fail(string message) =>
        new() { Success = false, Message = message };
}

/// <summary>
/// Standard API response with data
/// </summary>
public record ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public new static ApiResponse<T> Fail(string message) =>
        new() { Success = false, Message = message };
}

/// <summary>
/// Paginated API response
/// </summary>
public record PaginatedResponse<T> : ApiResponse<IReadOnlyList<T>>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}