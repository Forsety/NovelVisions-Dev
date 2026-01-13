using System.ComponentModel.DataAnnotations;

namespace NovelVision.Services.Catalog.API.Models.Requests
{
    public class UpdateAuthorRequest
    {
        [Required]
        [MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Biography { get; set; }

        public Dictionary<string, string>? SocialLinks { get; set; }
    }

}
