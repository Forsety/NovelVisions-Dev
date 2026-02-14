using System.ComponentModel.DataAnnotations;

namespace NovelVision.Services.Catalog.API.Models.Requests
{
    public class UpdateBookRequest
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Subtitle { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Publisher { get; set; }

        public DateTime? PublicationDate { get; set; }

        [MaxLength(50)]
        public string? Edition { get; set; }

        [MaxLength(5, ErrorMessage = "Maximum 5 genres allowed")]
        public List<string>? Genres { get; set; }

        [MaxLength(10, ErrorMessage = "Maximum 10 tags allowed")]
        public List<string>? Tags { get; set; }
    }

}
