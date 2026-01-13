using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Identity.Client;
using NovelVision.Services.Catalog.Infrastructure.Identity.Models;

namespace NovelVision.Services.Catalog.Infrastructure.Identity.Services;

/// <summary>
/// Service for managing user authentication and authorization
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Authenticates a user
    /// </summary>
    Task<Models.AuthenticationResult> AuthenticateAsync(Models.LoginRequest request);

    /// <summary>
    /// Registers a new user
    /// </summary>
    Task<Models.AuthenticationResult> RegisterAsync(Models.RegisterRequest request);

    /// <summary>
    /// Refreshes an access token
    /// </summary>
    Task<Models.AuthenticationResult> RefreshTokenAsync(RefreshTokenRequest request);

    /// <summary>
    /// Revokes a refresh token
    /// </summary>
    Task<bool> RevokeTokenAsync(string token, string reason);

    /// <summary>
    /// Changes user password
    /// </summary>
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);

    /// <summary>
    /// Resets user password
    /// </summary>
    Task<bool> ResetPasswordAsync(Models.ResetPasswordRequest request);

    /// <summary>
    /// Sends password reset email
    /// </summary>
    Task<bool> ForgotPasswordAsync(string email);

    /// <summary>
    /// Confirms user email
    /// </summary>
    Task<bool> ConfirmEmailAsync(Guid userId, string token);

    /// <summary>
    /// Gets user by ID
    /// </summary>
    Task<UserDto?> GetUserByIdAsync(Guid userId);

    /// <summary>
    /// Gets user by email
    /// </summary>
    Task<UserDto?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Updates user profile
    /// </summary>
    Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);

    /// <summary>
    /// Assigns role to user
    /// </summary>
    Task<bool> AssignRoleAsync(Guid userId, string roleName);

    /// <summary>
    /// Removes role from user
    /// </summary>
    Task<bool> RemoveRoleAsync(Guid userId, string roleName);
}
