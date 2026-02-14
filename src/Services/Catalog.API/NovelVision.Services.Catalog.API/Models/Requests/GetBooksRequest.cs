// src/Services/Catalog.API/NovelVision.Services.Catalog.API/Models/Requests/GetBooksRequest.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovelVision.Services.Catalog.API.Models.Requests;

/// <summary>
/// Request model for getting books with filters
/// </summary>
public class GetBooksRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int? PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int? PageSize { get; set; } = 20;

    public Guid? AuthorId { get; set; }

    public string? Status { get; set; }

    public string? Genre { get; set; }

    [MaxLength(100)]
    public string? SearchTerm { get; set; }

    public string? Language { get; set; }

    [Range(0, int.MaxValue)]
    public int? MinPages { get; set; }

    [Range(0, int.MaxValue)]
    public int? MaxPages { get; set; }

    // === NEW FIELDS ===

    /// <summary>
    /// Filter by copyright status (PublicDomain, Copyrighted, Unknown)
    /// </summary>
    public string? CopyrightStatus { get; set; }

    /// <summary>
    /// Filter by source (Gutenberg, OpenLibrary, UserCreated)
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Only return free-to-use books
    /// </summary>
    public bool? IsFreeToUse { get; set; }

    /// <summary>
    /// Subject IDs to filter by
    /// </summary>
    public List<Guid>? SubjectIds { get; set; }

    /// <summary>
    /// Sort by field
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort descending
    /// </summary>
    public bool Descending { get; set; } = true;
}