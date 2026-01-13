using System;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Application.Common.Interfaces;

/// <summary>
/// Defines the contract for a service that provides information about the current user.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's unique identifier.
    /// Can be null if the user is not authenticated.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the current user's email.
    /// Can be null if the user is not authenticated.
    /// </summary>
    string? UserEmail { get; }

    /// <summary>
    /// If the current user is an author, gets the corresponding AuthorId.
    /// Can be null if the user is not an author or not authenticated.
    /// </summary>
    AuthorId? AuthorId { get; }

    /// <summary>
    /// Checks if the current user is in a specific role.
    /// </summary>
    /// <param name="roleName">The name of the role.</param>
    /// <returns>True if the user is in the role, otherwise false.</returns>
    bool IsInRole(string roleName);
}