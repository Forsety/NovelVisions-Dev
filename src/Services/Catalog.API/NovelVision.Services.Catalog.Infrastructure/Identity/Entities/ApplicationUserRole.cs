using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelVision.Services.Catalog.Infrastructure.Identity.Entities
{
    public class ApplicationUserRole : IdentityUserRole<Guid>
    {
        /// <summary>
        /// Date when the role was assigned
        /// </summary>
        public DateTimeOffset AssignedAt { get; set; }

        /// <summary>
        /// Date when the role expires (null = never)
        /// </summary>
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// User who assigned this role
        /// </summary>
        public Guid? AssignedById { get; set; }

        /// <summary>
        /// Navigation property to user
        /// </summary>
        public virtual ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Navigation property to role
        /// </summary>
        public virtual ApplicationRole Role { get; set; } = null!;
    }

}
