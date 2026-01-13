// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/Identity/Services/IdentityService.cs
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NovelVision.Services.Catalog.Application.Common.Interfaces;
using NovelVision.Services.Catalog.Infrastructure.Identity.Entities;
using NovelVision.Services.Catalog.Infrastructure.Identity.Models;
using NovelVision.Services.Catalog.Infrastructure.Identity.Persistence;
using NovelVision.Services.Catalog.Infrastructure.Services.Email;

namespace NovelVision.Services.Catalog.Infrastructure.Identity.Services;

/// <summary>
/// Implementation of identity service
/// </summary>
public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly ApplicationIdentityDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        ITokenService tokenService,
        ApplicationIdentityDbContext context,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<IdentityService> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AuthenticationResult> AuthenticateAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return AuthenticationResult.Failure("Invalid email or password");
        }

        if (!user.IsActive)
        {
            return AuthenticationResult.Failure("Account is inactive");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                return AuthenticationResult.Failure("Account is locked out");
            }
            if (result.IsNotAllowed)
            {
                return AuthenticationResult.Failure("Email not confirmed");
            }
            return AuthenticationResult.Failure("Invalid email or password");
        }

        // Update last login
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);

        // Generate tokens
        var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        refreshToken.UserId = user.Id;

        // Remove old refresh tokens
        await RemoveOldRefreshTokensAsync(user.Id);

        // Save refresh token
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} authenticated successfully", user.Id);

        return AuthenticationResult.Success(
            accessToken,
            refreshToken.Token,
            refreshToken.ExpiresAt,
            new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DisplayName = user.DisplayName,
                AvatarUrl = user.AvatarUrl,
                Roles = await _userManager.GetRolesAsync(user)
            });
    }

    public async Task<AuthenticationResult> RegisterAsync(RegisterRequest request)
    {
        // Check if user exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return AuthenticationResult.Failure("Email already registered");
        }

        // Create user
        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DisplayName = $"{request.FirstName} {request.LastName}".Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            EmailConfirmed = false
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return AuthenticationResult.Failure($"Registration failed: {errors}");
        }

        // Assign default role
        await _userManager.AddToRoleAsync(user, ApplicationRoles.Reader);

        // Generate email confirmation token
        var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // Send confirmation email (async, don't await)
        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendEmailConfirmationAsync(user.Email, confirmationToken, _configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email}", user.Email);
            }
        });

        // Generate tokens
        var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        refreshToken.UserId = user.Id;

        // Save refresh token
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("New user {UserId} registered successfully", user.Id);

        return AuthenticationResult.Success(
            accessToken,
            refreshToken.Token,
            refreshToken.ExpiresAt,
            new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DisplayName = user.DisplayName,
                Roles = new[] { ApplicationRoles.Reader }
            });
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            return AuthenticationResult.Failure("Invalid access token");
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var userGuid))
        {
            return AuthenticationResult.Failure("Invalid access token");
        }

        var user = await _userManager.FindByIdAsync(userGuid.ToString());
        if (user == null || !user.IsActive)
        {
            return AuthenticationResult.Failure("User not found or inactive");
        }

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userGuid);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            return AuthenticationResult.Failure("Invalid or expired refresh token");
        }

        // Rotate refresh token
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        newRefreshToken.UserId = user.Id;
        refreshToken.ReplacedByToken = newRefreshToken.Token;
        refreshToken.RevokedAt = DateTimeOffset.UtcNow;
        refreshToken.ReasonRevoked = "Token rotation";

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        // Generate new access token
        var newAccessToken = await _tokenService.GenerateAccessTokenAsync(user);

        return AuthenticationResult.Success(
            newAccessToken,
            newRefreshToken.Token,
            newRefreshToken.ExpiresAt,
            new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DisplayName = user.DisplayName,
                AvatarUrl = user.AvatarUrl,
                Roles = await _userManager.GetRolesAsync(user)
            });
    }

    public async Task<bool> RevokeTokenAsync(string token, string reason)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            return false;
        }

        refreshToken.RevokedAt = DateTimeOffset.UtcNow;
        refreshToken.ReasonRevoked = reason;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        return result.Succeeded;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Don't reveal that the user doesn't exist
            return true;
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        return result.Succeeded;
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Don't reveal that the user doesn't exist
            return true;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Send reset email (async, don't await)
        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendPasswordResetAsync(email, token, _configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            }
        });

        return true;
    }

    public async Task<bool> ConfirmEmailAsync(Guid userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            Biography = user.Biography,
            Roles = await _userManager.GetRolesAsync(user)
        };
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            Biography = user.Biography,
            Roles = await _userManager.GetRolesAsync(user)
        };
    }

    public async Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        user.FirstName = request.FirstName ?? user.FirstName;
        user.LastName = request.LastName ?? user.LastName;
        user.DisplayName = request.DisplayName ?? user.DisplayName;
        user.Biography = request.Biography ?? user.Biography;
        user.AvatarUrl = request.AvatarUrl ?? user.AvatarUrl;
        user.PreferredLanguage = request.PreferredLanguage ?? user.PreferredLanguage;
        user.TimeZone = request.TimeZone ?? user.TimeZone;
        user.ModifiedAt = DateTimeOffset.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> AssignRoleAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            return false;
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
    }

    public async Task<bool> RemoveRoleAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        return result.Succeeded;
    }

    private async Task RemoveOldRefreshTokensAsync(Guid userId)
    {
        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-30);
        var oldTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && (rt.ExpiresAt < cutoffDate || rt.RevokedAt != null))
            .ToListAsync();

        if (oldTokens.Any())
        {
            _context.RefreshTokens.RemoveRange(oldTokens);
        }
    }

   
}