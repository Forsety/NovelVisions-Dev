// src/Services/Visualization.API/NovelVision.Services.Visualization.API/Controllers/HealthController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NovelVision.Services.Visualization.API.Controllers;

/// <summary>
/// Health check endpoints
/// </summary>
[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public sealed class HealthController : ControllerBase
{
    /// <summary>
    /// Simple ping endpoint
    /// </summary>
    [HttpGet("/ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "pong", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Error endpoint for testing exception handling
    /// </summary>
    [HttpGet("/error")]
    public IActionResult Error()
    {
        return Problem(
            title: "An error occurred",
            detail: "An unexpected error occurred while processing your request",
            statusCode: StatusCodes.Status500InternalServerError);
    }
}