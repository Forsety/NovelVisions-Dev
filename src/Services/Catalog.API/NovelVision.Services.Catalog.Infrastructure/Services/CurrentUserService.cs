using Microsoft.AspNetCore.Http;
using NovelVision.Services.Catalog.Application.Common.Interfaces;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;
using System;
using System.Linq;
using System.Security.Claims;

namespace NovelVision.Services.Catalog.Infrastructure.Services;

/// <summary>
/// Service to get information about the currently authenticated user from the HttpContext.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var userIdClaim = User?.FindFirstValue("uid"); // 'uid' из нашего IdentityService
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? UserEmail => User?.FindFirstValue(ClaimTypes.Email);

    public AuthorId? AuthorId
    {
        get
        {
            var authorIdClaim = User?.FindFirstValue("authorId"); // Этот claim нужно будет добавить при генерации токена
            return Guid.TryParse(authorIdClaim, out var authorId) ? new AuthorId(authorId) : null;
        }
    }

    public bool IsInRole(string roleName)
    {
        return User?.IsInRole(roleName) ?? false;
    }
}