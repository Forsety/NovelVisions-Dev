using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelVision.Services.Catalog.Infrastructure.Identity.Entities
{
    public class ApplicationUserToken : IdentityUserToken<Guid>
    {
        /// <summary>
        /// Date when the token was created
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Date when the token expires
        /// </summary>
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// Navigation property to user
        /// </summary>
        public virtual ApplicationUser User { get; set; } = null!;
    }

}
