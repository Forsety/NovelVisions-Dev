using System;

namespace NovelVision.Services.Catalog.Infrastructure.Identity.Settings;

/// <summary>
/// JWT authentication settings
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// JWT secret key for token signing
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Token audience
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Access token expiration in minutes
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh token expiration in days
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// Indicates whether to validate the issuer
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Indicates whether to validate the audience
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Indicates whether to validate the lifetime
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// Indicates whether to validate the signing key
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// Clock skew in minutes for token expiration validation
    /// </summary>
    public int ClockSkewMinutes { get; set; } = 5;
}
