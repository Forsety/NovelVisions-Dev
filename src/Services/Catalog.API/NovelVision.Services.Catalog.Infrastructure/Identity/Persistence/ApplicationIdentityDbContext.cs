// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/Identity/Persistence/ApplicationIdentityDbContext.cs
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using NovelVision.Services.Catalog.Infrastructure.Identity.Entities;

namespace NovelVision.Services.Catalog.Infrastructure.Identity.Persistence;

/// <summary>
/// Identity database context for managing users, roles, and authentication
/// </summary>
public class ApplicationIdentityDbContext : IdentityDbContext<
    ApplicationUser,
    ApplicationRole,
    Guid,
    ApplicationUserClaim,
    ApplicationUserRole,
    ApplicationUserLogin,
    ApplicationRoleClaim,
    ApplicationUserToken>
{
    public ApplicationIdentityDbContext()
    {
        // Parameterless constructor for migrations
    }

    public ApplicationIdentityDbContext(DbContextOptions<ApplicationIdentityDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Refresh tokens for JWT authentication
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // This will be used only for migrations
            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=NovelVisionIdentity;Trusted_Connection=True;MultipleActiveResultSets=true");
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all configurations from the current assembly
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Customize the ASP.NET Identity model and override the defaults
        // Change table names
        builder.Entity<ApplicationUser>().ToTable("Users", "identity");
        builder.Entity<ApplicationRole>().ToTable("Roles", "identity");
        builder.Entity<ApplicationUserRole>().ToTable("UserRoles", "identity");
        builder.Entity<ApplicationUserClaim>().ToTable("UserClaims", "identity");
        builder.Entity<ApplicationRoleClaim>().ToTable("RoleClaims", "identity");
        builder.Entity<ApplicationUserLogin>().ToTable("UserLogins", "identity");
        builder.Entity<ApplicationUserToken>().ToTable("UserTokens", "identity");
        builder.Entity<RefreshToken>().ToTable("RefreshTokens", "identity");

        // Configure relationships
        builder.Entity<ApplicationUserRole>(userRole =>
        {
            userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

            userRole.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            userRole.HasOne(ur => ur.User)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        });

        // Configure User entity
        builder.Entity<ApplicationUser>(user =>
        {
            user.Property(u => u.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            user.Property(u => u.LastName)
                .HasMaxLength(100)
                .IsRequired();

            user.Property(u => u.DisplayName)
                .HasMaxLength(200)
                .IsRequired();

            user.Property(u => u.Biography)
                .HasMaxLength(1000);

            user.Property(u => u.AvatarUrl)
                .HasMaxLength(500);

            user.Property(u => u.PreferredLanguage)
                .HasMaxLength(10)
                .IsRequired()
                .HasDefaultValue("en-US");

            user.Property(u => u.TimeZone)
                .HasMaxLength(50)
                .IsRequired()
                .HasDefaultValue("UTC");

            user.HasIndex(u => u.Email)
                .IsUnique();

            user.HasIndex(u => u.NormalizedEmail);

            user.HasIndex(u => u.IsDeleted);

            user.HasQueryFilter(u => !u.IsDeleted);
        });

        // Configure Role entity
        builder.Entity<ApplicationRole>(role =>
        {
            role.Property(r => r.Description)
                .HasMaxLength(500);

            role.HasIndex(r => r.Priority);
            role.HasIndex(r => r.IsActive);
        });

        // Configure RefreshToken entity
        builder.Entity<RefreshToken>(token =>
        {
            token.HasKey(t => t.Id);

            token.Property(t => t.Token)
                .HasMaxLength(500)
                .IsRequired();

            token.Property(t => t.ReplacedByToken)
                .HasMaxLength(500);

            token.Property(t => t.ReasonRevoked)
                .HasMaxLength(200);

            token.HasOne(t => t.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            token.HasIndex(t => t.Token)
                .IsUnique();

            token.HasIndex(t => t.ExpiresAt);
        });

        // Configure UserClaim entity
        builder.Entity<ApplicationUserClaim>(claim =>
        {
            claim.HasOne(c => c.User)
                .WithMany(u => u.Claims)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure UserLogin entity
        builder.Entity<ApplicationUserLogin>(login =>
        {
            login.HasOne(l => l.User)
                .WithMany(u => u.Logins)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure UserToken entity
        builder.Entity<ApplicationUserToken>(token =>
        {
            token.HasOne(t => t.User)
                .WithMany(u => u.Tokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure RoleClaim entity
        builder.Entity<ApplicationRoleClaim>(claim =>
        {
            claim.HasOne(c => c.Role)
                .WithMany(r => r.RoleClaims)
                .HasForeignKey(c => c.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<ApplicationUser>();
        var utcNow = DateTimeOffset.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = utcNow;
            }
        }

        var roleEntries = ChangeTracker.Entries<ApplicationRole>();
        foreach (var entry in roleEntries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = utcNow;
            }
        }
    }
}

/// <summary>
/// Design-time factory for ApplicationIdentityDbContext
/// </summary>
public class ApplicationIdentityDbContextFactory : IDesignTimeDbContextFactory<ApplicationIdentityDbContext>
{
    public ApplicationIdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationIdentityDbContext>();

        // Try to get configuration
        IConfigurationRoot configuration = null;
        try
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();
        }
        catch
        {
            // If configuration fails, use default connection string
        }

        var connectionString = configuration?.GetConnectionString("IdentityConnection")
            ?? configuration?.GetConnectionString("DefaultConnection")
            ?? "Data Source=LAPTOP-OHNFI7TT;Initial Catalog=NovelVisionIdentity;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationIdentityDbContext(optionsBuilder.Options);
    }
}