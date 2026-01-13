// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/Identity/Entities/ApplicationUser.cs
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace NovelVision.Services.Catalog.Infrastructure.Identity.Entities;

/// <summary>
/// Represents an application user with extended properties
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// User's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User's display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// User's avatar URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// User's biography
    /// </summary>
    public string? Biography { get; set; }

    /// <summary>
    /// Date when the user was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Date when the user was last modified
    /// </summary>
    public DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>
    /// Date when the user last logged in
    /// </summary>
    public DateTimeOffset? LastLoginAt { get; set; }

    /// <summary>
    /// Indicates if the user is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates if the user is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Date when the user was deleted
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// User's preferred language
    /// </summary>
    public string PreferredLanguage { get; set; } = "en-US";

    /// <summary>
    /// User's timezone
    /// </summary>
    public string TimeZone { get; set; } = "UTC";

    /// <summary>
    /// User's refresh tokens for JWT authentication
    /// </summary>
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>
    /// User's claims
    /// </summary>
    public virtual ICollection<ApplicationUserClaim> Claims { get; set; } = new List<ApplicationUserClaim>();

    /// <summary>
    /// User's roles
    /// </summary>
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();

    /// <summary>
    /// User's logins
    /// </summary>
    public virtual ICollection<ApplicationUserLogin> Logins { get; set; } = new List<ApplicationUserLogin>();

    /// <summary>
    /// User's tokens
    /// </summary>
    public virtual ICollection<ApplicationUserToken> Tokens { get; set; } = new List<ApplicationUserToken>();

    /// <summary>
    /// Gets the user's full name
    /// </summary>
    public string GetFullName() => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Gets the user's initials
    /// </summary>
    public string GetInitials()
    {
        var initials = string.Empty;
        if (!string.IsNullOrWhiteSpace(FirstName))
            initials += FirstName[0];
        if (!string.IsNullOrWhiteSpace(LastName))
            initials += LastName[0];
        return initials.ToUpper();
    }
}

/// <summary>
/// Represents a refresh token for JWT authentication
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;
}