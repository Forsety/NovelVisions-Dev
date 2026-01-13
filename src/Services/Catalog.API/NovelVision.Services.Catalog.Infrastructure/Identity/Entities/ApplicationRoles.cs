using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelVision.Services.Catalog.Infrastructure.Identity.Entities
{
    public static class ApplicationRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Author = "Author";
        public const string Editor = "Editor";
        public const string Moderator = "Moderator";
        public const string Reader = "Reader";

        public static readonly string[] AllRoles =
        {
        SuperAdmin,
        Admin,
        Author,
        Editor,
        Moderator,
        Reader
    };

        public static readonly Dictionary<string, string> RoleDescriptions = new()
    {
        { SuperAdmin, "Full system access with all permissions" },
        { Admin, "Administrative access to manage users and content" },
        { Author, "Can create and manage own books and content" },
        { Editor, "Can edit and review books from authors" },
        { Moderator, "Can moderate comments and user content" },
        { Reader, "Basic access to read books and content" }
    };

        public static readonly Dictionary<string, int> RolePriorities = new()
    {
        { SuperAdmin, 1000 },
        { Admin, 900 },
        { Editor, 800 },
        { Moderator, 700 },
        { Author, 600 },
        { Reader, 100 }
    };
    }

}
