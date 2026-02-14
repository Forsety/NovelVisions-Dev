// src/Services/Catalog.API/NovelVision.Services.Catalog.API/Models/Requests/ImportGutenbergBookRequest.cs
using System.ComponentModel.DataAnnotations;

namespace NovelVision.Services.Catalog.API.Models.Requests;

/// <summary>
/// Request model for importing a single book from Project Gutenberg
/// </summary>
public class ImportGutenbergBookRequest
{
    /// <summary>
    /// Whether to import the full text content
    /// </summary>
    public bool ImportFullText { get; set; } = true;

    /// <summary>
    /// Number of words per page when splitting text
    /// </summary>
    [Range(100, 1000)]
    public int WordsPerPage { get; set; } = 300;

    /// <summary>
    /// Create author if not exists in database
    /// </summary>
    public bool CreateAuthorIfNotExists { get; set; } = true;

    /// <summary>
    /// Create subject categories if not exist
    /// </summary>
    public bool CreateSubjectsIfNotExist { get; set; } = true;

    /// <summary>
    /// Skip if book already exists in database
    /// </summary>
    public bool SkipIfExists { get; set; } = true;
}