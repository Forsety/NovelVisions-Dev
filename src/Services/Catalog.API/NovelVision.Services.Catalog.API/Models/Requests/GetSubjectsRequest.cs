// src/Services/Catalog.API/NovelVision.Services.Catalog.API/Models/Requests/GetSubjectsRequest.cs
using System.ComponentModel.DataAnnotations;

namespace NovelVision.Services.Catalog.API.Models.Requests;

/// <summary>
/// Request model for getting subjects
/// </summary>
public class GetSubjectsRequest
{
    /// <summary>
    /// Filter by subject type (Topic, Bookshelf, Genre, Era, Location)
    /// </summary>
    [MaxLength(50)]
    public string? Type { get; set; }

    /// <summary>
    /// Only return root subjects (no parent)
    /// </summary>
    public bool? OnlyRoot { get; set; }

    /// <summary>
    /// Include subjects with no books
    /// </summary>
    public bool? IncludeEmpty { get; set; }
}