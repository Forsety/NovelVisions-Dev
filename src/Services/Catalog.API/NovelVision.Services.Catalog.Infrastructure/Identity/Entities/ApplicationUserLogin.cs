using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelVision.Services.Catalog.Infrastructure.Identity.Entities
{
    public class ApplicationUserLogin : IdentityUserLogin<Guid>
    {
        /// <summary>
        /// Date when the login was created
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Navigation property to user
        /// </summary>
        public virtual ApplicationUser User { get; set; } = null!;
    }

}
