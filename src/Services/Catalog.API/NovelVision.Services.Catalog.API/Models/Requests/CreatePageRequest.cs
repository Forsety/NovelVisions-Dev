namespace NovelVision.Services.Catalog.API.Models.Requests;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class CreatePageRequest
{
    [Required]
    [MinLength(10, ErrorMessage = "Content must be at least 10 characters")]
    [MaxLength(50000, ErrorMessage = "Content cannot exceed 50000 characters")]
    public string Content { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int? PageNumber { get; set; } // If null, will be auto-calculated

    public Dictionary<string, object>? VisualizationSettings { get; set; }

    public bool? GenerateVisualization { get; set; } = true;
}

public class UpdatePageRequest
{
    [Required]
    [MinLength(10, ErrorMessage = "Content must be at least 10 characters")]
    [MaxLength(50000, ErrorMessage = "Content cannot exceed 50000 characters")]
    public string Content { get; set; } = string.Empty;

    public Dictionary<string, object>? VisualizationSettings { get; set; }

    public bool? RegenerateVisualization { get; set; } = false;
}
