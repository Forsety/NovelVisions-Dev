using System.ComponentModel.DataAnnotations;

namespace NovelVision.Services.Catalog.API.Models.Requests;

public class CreateChapterRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Summary { get; set; }

    [Range(1, int.MaxValue)]
    public int? OrderIndex { get; set; } // If null, will be auto-calculated
}

public class UpdateChapterRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Summary { get; set; }
}

public class ReorderChapterRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Order index must be greater than 0")]
    public int NewOrderIndex { get; set; }
}