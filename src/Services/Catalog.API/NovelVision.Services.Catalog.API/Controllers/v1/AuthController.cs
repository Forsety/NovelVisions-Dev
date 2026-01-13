// src/Services/Catalog.API/NovelVision.Services.Catalog.API/Controllers/v1/AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovelVision.Services.Catalog.Infrastructure.Identity.Models;
using NovelVision.Services.Catalog.Infrastructure.Identity.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace NovelVision.Services.Catalog.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IIdentityService identityService,
        ILogger<AuthController> logger)
    {
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
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
            _logger.LogWarning("Failed registration for {Email}: {Error}", request.Email, result.Error);
            return BadRequest(new { error = result.Error });
        }

        _logger.LogInformation("New user {Email} registered successfully", request.Email);
        return Ok(result);
    }

    /// <summary>
    /// Refreshes access token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Refresh access token", Description = "Generates new access token using refresh token")]
    [SwaggerResponse(200, "Token refreshed", typeof(AuthenticationResult))]
    [SwaggerResponse(401, "Invalid or expired refresh token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _identityService.RefreshTokenAsync(request);

        if (!result.Succeeded)
        {
            return Unauthorized(new { error = result.Error });
        }

        return Ok(result);
    }

    /// <summary>
    /// Revokes refresh token
    /// </summary>
    [HttpPost("revoke")]
    [Authorize]
    [SwaggerOperation(Summary = "Revoke refresh token", Description = "Invalidates a refresh token")]
    [SwaggerResponse(204, "Token revoked successfully")]
    [SwaggerResponse(400, "Invalid request")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        var result = await _identityService.RevokeTokenAsync(request.RefreshToken, "User requested");

        if (!result)
        {
            return BadRequest(new { error = "Failed to revoke token" });
        }

        return NoContent();
    }

    /// <summary>
    /// Changes user password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [SwaggerOperation(Summary = "Change password", Description = "Changes the current user's password")]
    [SwaggerResponse(204, "Password changed successfully")]
    [SwaggerResponse(400, "Invalid request or current password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        var result = await _identityService.ChangePasswordAsync(userGuid, request);

        if (!result)
        {
            return BadRequest(new { error = "Failed to change password. Check your current password." });
        }

        _logger.LogInformation("User {UserId} changed password successfully", userId);
        return NoContent();
    }

    /// <summary>
    /// Initiates password reset
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Forgot password", Description = "Sends password reset email")]
    [SwaggerResponse(204, "Reset email sent if account exists")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _identityService.ForgotPasswordAsync(request.Email);

        // Always return success to prevent email enumeration
        return NoContent();
    }

    /// <summary>
    /// Resets password with token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Reset password", Description = "Resets password using token from email")]
    [SwaggerResponse(204, "Password reset successfully")]
    [SwaggerResponse(400, "Invalid or expired token")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _identityService.ResetPasswordAsync(request);

        if (!result)
        {
            return BadRequest(new { error = "Invalid or expired reset token" });
        }

        return NoContent();
    }

    /// <summary>
    /// Confirms email address
    /// </summary>
    [HttpGet("confirm-email")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Confirm email", Description = "Confirms user email with token")]
    [SwaggerResponse(200, "Email confirmed successfully")]
    [SwaggerResponse(400, "Invalid or expired token")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token, [FromQuery] string email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        {
            return BadRequest(new { error = "Token and email are required" });
        }

        var user = await _identityService.GetUserByEmailAsync(email);
        if (user == null)
        {
            return BadRequest(new { error = "Invalid request" });
        }

        var result = await _identityService.ConfirmEmailAsync(user.Id, token);

        if (!result)
        {
            return BadRequest(new { error = "Invalid or expired confirmation token" });
        }

        return Ok(new { message = "Email confirmed successfully" });
    }

    /// <summary>
    /// Gets current user profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [SwaggerOperation(Summary = "Get profile", Description = "Gets the current user's profile")]
    [SwaggerResponse(200, "Profile retrieved", typeof(UserDto))]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        var user = await _identityService.GetUserByIdAsync(userGuid);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    /// <summary>
    /// Updates user profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    [SwaggerOperation(Summary = "Update profile", Description = "Updates the current user's profile")]
    [SwaggerResponse(204, "Profile updated successfully")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        var result = await _identityService.UpdateProfileAsync(userGuid, request);

        if (!result)
        {
            return BadRequest(new { error = "Failed to update profile" });
        }

        return NoContent();
    }
}

public class RevokeTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}