// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/Identity/Persistence/IdentityDataSeeder.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NovelVision.Services.Catalog.Infrastructure.Identity.Entities;

namespace NovelVision.Services.Catalog.Infrastructure.Identity.Persistence;

/// <summary>
/// Seeds initial data for Identity database
/// </summary>
public class IdentityDataSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationIdentityDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ApplicationIdentityDbContext context,
        IConfiguration configuration,
        ILogger<IdentityDataSeeder> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Seeds initial roles and users
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            // Ensure database is created and migrated
            await _context.Database.MigrateAsync();
            _logger.LogInformation("Database migration completed");

            // Seed roles first
            await SeedRolesAsync();

            // Then seed users
            await SeedUsersAsync();

            // Seed additional test data if in development
            if (_configuration.GetValue<bool>("Identity:SeedTestData", false))
            {
                await SeedTestDataAsync();
            }

            _logger.LogInformation("Identity data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding identity data");
            throw;
        }
    }

    /// <summary>
    /// Seeds all system roles
    /// </summary>
    private async Task SeedRolesAsync()
    {
        _logger.LogInformation("Starting role seeding...");

        foreach (var roleName in ApplicationRoles.AllRoles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var role = new ApplicationRole
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    Description = ApplicationRoles.RoleDescriptions.GetValueOrDefault(roleName, string.Empty),
                    Priority = ApplicationRoles.RolePriorities.GetValueOrDefault(roleName, 0),
                    IsSystemRole = true,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Created role: {RoleName}", roleName);

                    // Add specific claims for each role
                    await AddRoleClaimsAsync(role);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create role {RoleName}: {Errors}", roleName, errors);
                }
            }
            else
            {
                _logger.LogInformation("Role {RoleName} already exists, skipping", roleName);
            }
        }
    }

    /// <summary>
    /// Adds claims to a role based on its permissions
    /// </summary>
    private async Task AddRoleClaimsAsync(ApplicationRole role)
    {
        var claims = role.Name switch
        {
            ApplicationRoles.SuperAdmin => new List<Claim>
            {
                new("permission", "system.manage"),
                new("permission", "users.manage"),
                new("permission", "users.delete"),
                new("permission", "roles.manage"),
                new("permission", "books.manage"),
                new("permission", "books.delete.any"),
                new("permission", "authors.manage"),
                new("permission", "visualization.manage"),
                new("permission", "reports.full"),
                new("permission", "settings.manage"),
                new("permission", "audit.view")
            },

            ApplicationRoles.Admin => new List<Claim>
            {
                new("permission", "users.manage"),
                new("permission", "users.suspend"),
                new("permission", "books.manage"),
                new("permission", "books.approve"),
                new("permission", "authors.manage"),
                new("permission", "reports.view"),
                new("permission", "moderation.full"),
                new("permission", "content.manage")
            },

            ApplicationRoles.Author => new List<Claim>
            {
                new("permission", "books.create"),
                new("permission", "books.edit.own"),
                new("permission", "books.delete.own"),
                new("permission", "books.publish.own"),
                new("permission", "chapters.manage.own"),
                new("permission", "visualization.enable"),
                new("permission", "analytics.view.own"),
                new("permission", "comments.respond")
            },

            ApplicationRoles.Editor => new List<Claim>
            {
                new("permission", "books.edit.any"),
                new("permission", "books.review"),
                new("permission", "books.approve"),
                new("permission", "chapters.edit.any"),
                new("permission", "content.moderate"),
                new("permission", "comments.moderate"),
                new("permission", "reports.content")
            },

            ApplicationRoles.Moderator => new List<Claim>
            {
                new("permission", "comments.moderate"),
                new("permission", "comments.delete"),
                new("permission", "users.warn"),
                new("permission", "users.suspend"),
                new("permission", "content.flag"),
                new("permission", "content.hide"),
                new("permission", "reports.moderation")
            },

            ApplicationRoles.Reader => new List<Claim>
            {
                new("permission", "books.read"),
                new("permission", "books.bookmark"),
                new("permission", "comments.create"),
                new("permission", "comments.edit.own"),
                new("permission", "profile.manage.own"),
                new("permission", "library.manage.own")
            },

            _ => new List<Claim>()
        };

        foreach (var claim in claims)
        {
            var result = await _roleManager.AddClaimAsync(role, claim);
            if (result.Succeeded)
            {
                _logger.LogDebug("Added claim {ClaimType}:{ClaimValue} to role {RoleName}",
                    claim.Type, claim.Value, role.Name);
            }
            else
            {
                _logger.LogWarning("Failed to add claim {ClaimType}:{ClaimValue} to role {RoleName}",
                    claim.Type, claim.Value, role.Name);
            }
        }
    }

    /// <summary>
    /// Seeds default users
    /// </summary>
    private async Task SeedUsersAsync()
    {
        _logger.LogInformation("Starting user seeding...");

        // Seed Super Admin from configuration
        var adminEmail = _configuration["Identity:SuperAdmin:Email"] ?? "admin@novelvision.com";
        var adminPassword = _configuration["Identity:SuperAdmin:Password"];

        if (string.IsNullOrEmpty(adminPassword))
        {
            _logger.LogWarning("Super admin password not configured, using default (CHANGE THIS IN PRODUCTION!)");
            adminPassword = "Admin123!@#";
        }

        await CreateUserAsync(
            email: adminEmail,
            password: adminPassword,
            firstName: "System",
            lastName: "Administrator",
            displayName: "Super Admin",
            role: ApplicationRoles.SuperAdmin,
            emailConfirmed: true,
            isSystemUser: true
        );

        // Seed additional configured users
        var seedUsers = _configuration.GetSection("Identity:SeedUsers").Get<List<SeedUserConfig>>();
        if (seedUsers != null)
        {
            foreach (var seedUser in seedUsers)
            {
                await CreateUserAsync(
                    email: seedUser.Email,
                    password: seedUser.Password,
                    firstName: seedUser.FirstName,
                    lastName: seedUser.LastName,
                    displayName: seedUser.DisplayName ?? $"{seedUser.FirstName} {seedUser.LastName}",
                    role: seedUser.Role,
                    emailConfirmed: seedUser.EmailConfirmed,
                    isSystemUser: false
                );
            }
        }
    }

    /// <summary>
    /// Seeds test data for development environment
    /// </summary>
    private async Task SeedTestDataAsync()
    {
        _logger.LogInformation("Seeding test data for development...");

        // Test Author
        await CreateUserAsync(
            email: "author@test.com",
            password: "Author123!@#",
            firstName: "John",
            lastName: "Writer",
            displayName: "John Writer",
            role: ApplicationRoles.Author,
            emailConfirmed: true,
            biography: "Professional writer with 10+ years of experience in fiction and non-fiction."
        );

        // Test Editor
        await CreateUserAsync(
            email: "editor@test.com",
            password: "Editor123!@#",
            firstName: "Sarah",
            lastName: "Editor",
            displayName: "Sarah Editor",
            role: ApplicationRoles.Editor,
            emailConfirmed: true,
            biography: "Senior editor specializing in fantasy and science fiction literature."
        );

        // Test Moderator
        await CreateUserAsync(
            email: "moderator@test.com",
            password: "Moderator123!@#",
            firstName: "Mike",
            lastName: "Moderator",
            displayName: "Mike Moderator",
            role: ApplicationRoles.Moderator,
            emailConfirmed: true
        );

        // Test Readers
        var readerNames = new[]
        {
            ("Alice", "Reader", "alice@test.com"),
            ("Bob", "Bookworm", "bob@test.com"),
            ("Charlie", "Page", "charlie@test.com"),
            ("Diana", "Novel", "diana@test.com"),
            ("Eve", "Story", "eve@test.com")
        };

        foreach (var (firstName, lastName, email) in readerNames)
        {
            await CreateUserAsync(
                email: email,
                password: "Reader123!@#",
                firstName: firstName,
                lastName: lastName,
                displayName: $"{firstName} {lastName}",
                role: ApplicationRoles.Reader,
                emailConfirmed: true
            );
        }

        _logger.LogInformation("Test data seeding completed");
    }

    /// <summary>
    /// Creates a user with specified parameters
    /// </summary>
    private async Task CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string displayName,
        string role,
        bool emailConfirmed = false,
        string? biography = null,
        bool isSystemUser = false)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                _logger.LogInformation("User {Email} already exists, skipping", email);

                // Ensure user has the correct role
                if (!await _userManager.IsInRoleAsync(existingUser, role))
                {
                    await _userManager.AddToRoleAsync(existingUser, role);
                    _logger.LogInformation("Added role {Role} to existing user {Email}", role, email);
                }
                return;
            }

            // Create new user
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                UserName = email,
                NormalizedUserName = email.ToUpperInvariant(),
                FirstName = firstName,
                LastName = lastName,
                DisplayName = displayName,
                Biography = biography,
                EmailConfirmed = emailConfirmed,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = !isSystemUser, // System users cannot be locked out
                AccessFailedCount = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true,
                IsDeleted = false,
                PreferredLanguage = "en-US",
                TimeZone = "UTC"
            };

            // Create user with password
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Add to role
                var roleResult = await _userManager.AddToRoleAsync(user, role);

                if (roleResult.Succeeded)
                {
                    _logger.LogInformation("Created user {Email} with role {Role}", email, role);

                    // Add additional claims for specific users
                    if (isSystemUser)
                    {
                        await _userManager.AddClaimAsync(user, new Claim("account_type", "system"));
                    }
                }
                else
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to add role {Role} to user {Email}: {Errors}", role, email, errors);
                }
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user {Email}: {Errors}", email, errors);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Email}", email);
        }
    }

    /// <summary>
    /// Configuration model for seed users
    /// </summary>
    private class SeedUserConfig
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string Role { get; set; } = ApplicationRoles.Reader;
        public bool EmailConfirmed { get; set; }
    }
}