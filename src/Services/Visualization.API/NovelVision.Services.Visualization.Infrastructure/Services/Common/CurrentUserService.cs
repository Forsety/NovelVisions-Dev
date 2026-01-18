// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Services/Common/CurrentUserService.cs

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NovelVision.Services.Visualization.Application.Interfaces;

namespace NovelVision.Services.Visualization.Infrastructure.Services.Common;

/// <summary>
/// Сервис для получения информации о текущем пользователе
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");

            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User?
        .FindAll(ClaimTypes.Role)
        .Select(c => c.Value)
        ?? Enumerable.Empty<string>();

    public bool IsAdmin => Roles.Contains("Admin");

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}