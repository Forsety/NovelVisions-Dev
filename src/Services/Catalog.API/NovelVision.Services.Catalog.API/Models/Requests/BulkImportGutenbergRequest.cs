// src/Services/Catalog.API/NovelVision.Services.Catalog.API/Models/Requests/BulkImportGutenbergRequest.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovelVision.Services.Catalog.API.Models.Requests;

/// <summary>
/// Request model for bulk importing books from Project Gutenberg
/// </summary>
public class BulkImportGutenbergRequest
{
    /// <summary>
    /// Specific Gutenberg IDs to import (optional)
    /// </summary>
    public List<int>? GutenbergIds { get; set; }

    /// <summary>
    /// Search criteria for finding books to import (alternative to GutenbergIds)
    /// </summary>
    public GutenbergSearchCriteria? SearchCriteria { get; set; }

    /// <summary>
    /// Maximum number of books to import
    /// </summary>
    [Range(1, 1000)]
    public int MaxBooks { get; set; } = 100;

    /// <summary>
    /// Whether to import full text content
    /// </summary>
    public bool ImportFullText { get; set; } = true;

    /// <summary>
    /// Words per page when splitting text
    /// </summary>
    [Range(100, 1000)]
    public int WordsPerPage { get; set; } = 300;

    /// <summary>
    /// Skip books that already exist
    /// </summary>
    public bool SkipExisting { get; set; } = true;

    /// <summary>
    /// Continue importing even if some books fail
    /// </summary>
    public bool ContinueOnError { get; set; } = true;

    /// <summary>
    /// Delay between requests in milliseconds (rate limiting)
    /// </summary>
    [Range(0, 10000)]
    public int DelayBetweenRequests { get; set; } = 500;
}

/// <summary>
/// Search criteria for Gutenberg books
/// </summary>
public class GutenbergSearchCriteria
{
    public string? Search { get; set; }
    public List<string>? Languages { get; set; }
    public string? Topic { get; set; }
    public int? AuthorYearStart { get; set; }
    public int? AuthorYearEnd { get; set; }
    public bool? Copyright { get; set; }
}