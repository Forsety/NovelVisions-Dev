using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelVision.Services.Catalog.Infrastructure.Identity.Entities
{
    public class ApplicationRoleClaim : IdentityRoleClaim<Guid>
    {
        /// <summary>
        /// Date when the claim was created
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Navigation property to role
        /// </summary>
        public virtual ApplicationRole Role { get; set; } = null!;
    }

}
