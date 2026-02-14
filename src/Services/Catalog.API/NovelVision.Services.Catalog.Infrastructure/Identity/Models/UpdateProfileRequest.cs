using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelVision.Services.Catalog.Infrastructure.Identity.Models
{
    public class UpdateProfileRequest
    {
        [MinLength(2)]
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MinLength(2)]
        [MaxLength(100)]
        public string? LastName { get; set; }

        [MinLength(2)]
        [MaxLength(200)]
        public string? DisplayName { get; set; }

        [MaxLength(1000)]
        public string? Biography { get; set; }

        [Url]
        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        [MaxLength(10)]
        public string? PreferredLanguage { get; set; }

        [MaxLength(50)]
        public string? TimeZone { get; set; }
    }

}
