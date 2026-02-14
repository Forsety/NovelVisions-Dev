// src/Services/Catalog.API/NovelVision.Services.Catalog.API/Models/Requests/SearchGutenbergRequest.cs
using System.ComponentModel.DataAnnotations;

namespace NovelVision.Services.Catalog.API.Models.Requests;

/// <summary>
/// Request model for searching Project Gutenberg
/// </summary>
public class SearchGutenbergRequest
{
    /// <summary>
    /// Search term (searches titles and authors)
    /// </summary>
    [MaxLength(200)]
    public string? Search { get; set; }

    /// <summary>
    /// Filter by languages (comma-separated, e.g., "en,fr")
    /// </summary>
    [MaxLength(50)]
    public string? Languages { get; set; }

    /// <summary>
    /// Filter by topic/subject
    /// </summary>
    [MaxLength(100)]
    public string? Topic { get; set; }

    /// <summary>
    /// Filter by author birth year (start)
    /// </summary>
    public int? AuthorYearStart { get; set; }

    /// <summary>
    /// Filter by author birth year (end)
    /// </summary>
    public int? AuthorYearEnd { get; set; }

    /// <summary>
    /// Filter by copyright status (true = copyrighted, false = public domain)
    /// </summary>
    public bool? Copyright { get; set; }

    /// <summary>
    /// Page number
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Sort order: "popular" (default), "ascending", "descending"
    /// </summary>
    [RegularExpression("^(popular|ascending|descending)$")]
    public string Sort { get; set; } = "popular";
}