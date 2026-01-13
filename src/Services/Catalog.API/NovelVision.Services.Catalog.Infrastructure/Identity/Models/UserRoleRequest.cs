using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelVision.Services.Catalog.Infrastructure.Identity.Models
{
    public class UserRoleRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string RoleName { get; set; } = string.Empty;

        public DateTimeOffset? ExpiresAt { get; set; }
    }

}
