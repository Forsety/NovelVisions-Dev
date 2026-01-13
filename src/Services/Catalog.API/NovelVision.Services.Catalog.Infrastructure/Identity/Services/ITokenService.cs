using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using NovelVision.Services.Catalog.Infrastructure.Identity.Entities;

namespace NovelVision.Services.Catalog.Infrastructure.Identity.Services;

/// <summary>
/// Service for generating and validating JWT tokens
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates an access token for the user
    /// </summary>
    Task<string> GenerateAccessTokenAsync(ApplicationUser user);

    /// <summary>
    /// Generates a refresh token
    /// </summary>
    RefreshToken GenerateRefreshToken();

    /// <summary>
    /// Gets the principal from an expired token
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    /// <summary>
    /// Validates a token
    /// </summary>
    Task<bool> ValidateTokenAsync(string token);

    /// <summary>
    /// Gets claims for a user
    /// </summary>
    Task<IList<Claim>> GetUserClaimsAsync(ApplicationUser user);
}
