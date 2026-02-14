// src/Services/Catalog.API/NovelVision.Services.Catalog.API/Controllers/v1/AuthController.cs
// FIXED: BecomeAuthor now creates Author record in Catalog database

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovelVision.Services.Catalog.API.Models.Responses;
using NovelVision.Services.Catalog.Application.Commands.Authors;
using NovelVision.Services.Catalog.Application.Common.Interfaces;
using NovelVision.Services.Catalog.Application.Queries.Authors;  // <-- добавь это
using NovelVision.Services.Catalog.Infrastructure.Identity.Entities;
using NovelVision.Services.Catalog.Infrastructure.Identity.Models;
using NovelVision.Services.Catalog.Infrastructure.Identity.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using NovelVision.Services.Catalog.Application.Queries.Books;  // <-- GetAuthorByUserIdQuery здесь!


namespace NovelVision.Services.Catalog.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IIdentityService identityService,
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<AuthController> logger)
    {
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Authenticates a user
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Login user", Description = "Authenticates a user and returns JWT tokens")]
    [SwaggerResponse(200, "Successfully authenticated", typeof(AuthenticationResult))]
    [SwaggerResponse(400, "Invalid request")]
    [SwaggerResponse(401, "Invalid credentials")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _identityService.AuthenticateAsync(request);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed login attempt for {Email}", request.Email);
            return Unauthorized(new { error = result.Error });
        }

        _logger.LogInformation("User {Email} logged in successfully", request.Email);
        return Ok(result);
    }

    /// <summary>
    /// Registers a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Register new user", Description = "Creates a new user account")]
    [SwaggerResponse(200, "Successfully registered", typeof(AuthenticationResult))]
    [SwaggerResponse(400, "Invalid request or email already exists")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _identityService.RegisterAsync(request);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed registration attempt for {Email}: {Error}", request.Email, result.Error);
            return BadRequest(new { error = result.Error, errors = result.Errors });
        }

        _logger.LogInformation("New user {Email} registered successfully", request.Email);
        return Ok(result);
    }

    /// <summary>
    /// Refreshes an access token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Refresh token", Description = "Refreshes the access token using a valid refresh token")]
    [SwaggerResponse(200, "Token refreshed successfully", typeof(AuthenticationResult))]
    [SwaggerResponse(400, "Invalid or expired refresh token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _identityService.RefreshTokenAsync(request);

        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result);
    }

    /// <summary>
    /// Revokes a refresh token
    /// </summary>
    [HttpPost("revoke-token")]
    [Authorize]
    [SwaggerOperation(Summary = "Revoke token", Description = "Revokes a refresh token")]
    [SwaggerResponse(200, "Token revoked successfully")]
    [SwaggerResponse(400, "Invalid token")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        var result = await _identityService.RevokeTokenAsync(request.Token, "User requested revocation");

        if (!result)
        {
            return BadRequest(new { error = "Invalid token" });
        }

        return Ok(new { message = "Token revoked successfully" });
    }

    /// <summary>
    /// Gets the current user's profile
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [SwaggerOperation(Summary = "Get current user", Description = "Returns the current authenticated user's profile")]
    [SwaggerResponse(200, "User profile retrieved", typeof(UserDto))]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var user = await _identityService.GetUserByIdAsync(userId.Value);
        if (user == null)
        {
            return NotFound(new { error = "User not found" });
        }

        return Ok(user);
    }

    /// <summary>
    /// Updates the current user's profile
    /// </summary>
    [HttpPut("me")]
    [Authorize]
    [SwaggerOperation(Summary = "Update profile", Description = "Updates the current user's profile")]
    [SwaggerResponse(200, "Profile updated successfully")]
    [SwaggerResponse(400, "Invalid request")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _identityService.UpdateProfileAsync(userId.Value, request);

        if (!result)
        {
            return BadRequest(new { error = "Failed to update profile" });
        }

        return Ok(new { message = "Profile updated successfully" });
    }

    /// <summary>
    /// Changes the current user's password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [SwaggerOperation(Summary = "Change password", Description = "Changes the current user's password")]
    [SwaggerResponse(200, "Password changed successfully")]
    [SwaggerResponse(400, "Invalid request or current password incorrect")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _identityService.ChangePasswordAsync(userId.Value, request);

        if (!result)
        {
            return BadRequest(new { error = "Failed to change password. Current password may be incorrect." });
        }

        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Initiates password reset
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Forgot password", Description = "Sends a password reset email")]
    [SwaggerResponse(200, "If email exists, reset instructions sent")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _identityService.ForgotPasswordAsync(request.Email);

        // Always return success to prevent email enumeration
        return Ok(new { message = "If the email exists, password reset instructions have been sent." });
    }

    /// <summary>
    /// Resets user password
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Reset password", Description = "Resets user password with token")]
    [SwaggerResponse(200, "Password reset successfully")]
    [SwaggerResponse(400, "Invalid or expired token")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _identityService.ResetPasswordAsync(request);

        if (!result)
        {
            return BadRequest(new { error = "Invalid or expired reset token" });
        }

        return Ok(new { message = "Password reset successfully" });
    }

    /// <summary>
    /// Upgrades current user to Author role and creates Author record
    /// </summary>
  [HttpPost("become-author")]
    [Authorize]
    [SwaggerOperation(Summary = "Become an author", Description = "Upgrades the current user to author role and creates author profile")]
    [SwaggerResponse(200, "Successfully upgraded to author")]
    [SwaggerResponse(400, "Already fully registered as author")]
    public async Task<IActionResult> BecomeAuthor([FromBody] BecomeAuthorRequest? request = null)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        try
        {
            // 1. Get user info
            var user = await _identityService.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            var higherRoles = new[] { ApplicationRoles.Author, ApplicationRoles.Editor, ApplicationRoles.Admin, ApplicationRoles.SuperAdmin };
            var hasAuthorRole = user.Roles.Any(r => higherRoles.Contains(r));

            // 2. Check if Author record already exists in [Catalog].[Authors]
            var existingAuthorQuery = new GetAuthorByUserIdQuery(userId.Value);
            var existingAuthorResult = await _mediator.Send(existingAuthorQuery);
            var hasAuthorRecord = existingAuthorResult.IsSucceeded && existingAuthorResult.Value != null;

            // 3. If BOTH role AND record exist → truly already an author, nothing to do
            if (hasAuthorRole && hasAuthorRecord)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "You are already an author or have higher privileges"
                });
            }

            // 4. Create Author record if missing (even if role already exists!)
            if (!hasAuthorRecord)
            {
                var displayName = request?.DisplayName ?? user.DisplayName ?? $"{user.FirstName} {user.LastName}".Trim();
                var biography = request?.Biography ?? "";

                var createAuthorCommand = new CreateAuthorCommand
                {
                    UserId = userId.Value,
                    DisplayName = displayName,
                    Email = user.Email,
                    Biography = biography,
                    SocialLinks = new Dictionary<string, string>()
                };

                var createResult = await _mediator.Send(createAuthorCommand);

                if (createResult.IsFailed)
                {
                    _logger.LogError("Failed to create author record for user {UserId}: {Errors}",
                        userId.Value, string.Join(", ", createResult.Errors.Select(e => e.Message)));

                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Failed to create author profile",
                        Errors = createResult.Errors.Select(e => e.Message).ToList()
                    });
                }

                _logger.LogInformation("Created author record {AuthorId} for user {UserId}",
                    createResult.Value.Id, userId.Value);
            }
            else
            {
                _logger.LogInformation("Author record already exists for user {UserId}, skipping creation", userId.Value);
            }

            // 5. Assign Author role if missing
            if (!hasAuthorRole)
            {
                var roleResult = await _identityService.AssignRoleAsync(userId.Value, ApplicationRoles.Author);
                if (!roleResult)
                {
                    _logger.LogWarning("Failed to assign Author role to user {UserId}", userId.Value);
                }
            }
            else
            {
                _logger.LogInformation("User {UserId} already has Author role, skipping role assignment", userId.Value);
            }

            _logger.LogInformation("User {UserId} successfully set up as Author (role={HasRole}, record={HasRecord}→created)",
                userId.Value, hasAuthorRole, !hasAuthorRecord);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "You are now an author! Please log out and log back in to refresh your session."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upgrading user {UserId} to author", userId.Value);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while upgrading to author"
            });
        }
    }
    /// <summary>
    /// Confirms user email
    /// </summary>
    [HttpGet("confirm-email")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Confirm email", Description = "Confirms user email with token")]
    [SwaggerResponse(200, "Email confirmed successfully")]
    [SwaggerResponse(400, "Invalid or expired token")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token)
    {
        var result = await _identityService.ConfirmEmailAsync(userId, token);

        if (!result)
        {
            return BadRequest(new { error = "Invalid or expired confirmation token" });
        }

        return Ok(new { message = "Email confirmed successfully" });
    }
}

// Request model for BecomeAuthor
public class BecomeAuthorRequest
{
    public string? DisplayName { get; set; }
    public string? Biography { get; set; }
}

public class RevokeTokenRequest
{
    public string Token { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}