// src/Services/Catalog.API/NovelVision.Services.Catalog.API/Models/Requests/SearchBooksAdvancedRequest.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovelVision.Services.Catalog.API.Models.Requests;

/// <summary>
/// Request model for advanced book search
/// </summary>
public class SearchBooksAdvancedRequest
{
    [MaxLength(200)]
    public string? SearchTerm { get; set; }

    public bool SearchInTitle { get; set; } = true;
    public bool SearchInDescription { get; set; } = true;
    public bool SearchInAuthor { get; set; } = false;

    /// <summary>
    /// Filter by subject IDs
    /// </summary>
    public List<Guid>? SubjectIds { get; set; }

    /// <summary>
    /// Filter by genres
    /// </summary>
    public List<string>? Genres { get; set; }

    /// <summary>
    /// Filter by languages (codes like "en", "fr")
    /// </summary>
    public List<string>? Languages { get; set; }

    /// <summary>
    /// Filter by copyright status
    /// </summary>
    public string? CopyrightStatus { get; set; }

    /// <summary>
    /// Filter by source (Gutenberg, OpenLibrary, UserCreated, etc.)
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Only show free-to-use books
    /// </summary>
    public bool? IsFreeToUse { get; set; }

    /// <summary>
    /// Only show imported books
    /// </summary>
    public bool? IsImported { get; set; }

    [Range(0, int.MaxValue)]
    public int? MinPageCount { get; set; }

    [Range(0, int.MaxValue)]
    public int? MaxPageCount { get; set; }

    [Range(0, int.MaxValue)]
    public int? MinDownloadCount { get; set; }

    public DateTime? PublishedAfter { get; set; }
    public DateTime? PublishedBefore { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort by: title, downloads, created, pages, rating
    /// </summary>
    [MaxLength(50)]
    public string? SortBy { get; set; }

    public bool Descending { get; set; } = true;
}