// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/Persistence/Configurations/AuthorConfiguration.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovelVision.Services.Catalog.Domain.Aggregates.AuthorAggregate;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Infrastructure.Persistence.Configurations;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("Authors", "Catalog");

        // Primary Key
        builder.HasKey(a => a.Id);

        // StronglyTypedId conversion
        builder.Property(a => a.Id)
            .HasConversion(
                v => v.Value,
                v => AuthorId.From(v))
            .ValueGeneratedNever()
            .HasColumnName("AuthorId");

        // Properties
        builder.Property(a => a.DisplayName)
            .HasMaxLength(100)
            .IsRequired()
            .HasColumnName("DisplayName");

        builder.Property(a => a.Email)
            .HasMaxLength(255)
            .IsRequired()
            .HasColumnName("Email");

        builder.Property(a => a.Biography)
            .HasMaxLength(2000)
            .HasColumnName("Biography");

        builder.Property(a => a.IsVerified)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnName("IsVerified");

        builder.Property(a => a.VerifiedAt)
            .HasColumnType("datetime2")
            .HasColumnName("VerifiedAt");

        // BookIds as JSON in SQL Server
        builder.Property(a => a.BookIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(
                    v.Select(id => id.Value).ToList(),
                    (System.Text.Json.JsonSerializerOptions?)null),
                v => DeserializeBookIds(v))
            .HasColumnType("nvarchar(max)")
            .HasColumnName("BookIds");

        // SocialLinks as JSON
        builder.Property(a => a.SocialLinks)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null)
                    ?? new Dictionary<string, string>())
            .HasColumnType("nvarchar(max)")
            .HasColumnName("SocialLinks");

        // Timestamps
        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasColumnName("CreatedAt");

        builder.Property(a => a.UpdatedAt)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasColumnName("UpdatedAt");

        // Indexes
        builder.HasIndex(a => a.Email)
            .IsUnique()
            .HasDatabaseName("UX_Authors_Email");

        builder.HasIndex(a => a.DisplayName)
            .HasDatabaseName("IX_Authors_DisplayName");

        builder.HasIndex(a => a.IsVerified)
            .HasDatabaseName("IX_Authors_IsVerified");

        // Ignore computed properties
        builder.Ignore(a => a.BookCount);
        builder.Ignore(a => a.HasBooks);
        builder.Ignore(a => a.DomainEvents);

        builder.Property(a => a.BirthYear)
            .HasColumnName("BirthYear");

        // DeathYear
        builder.Property(a => a.DeathYear)
            .HasColumnName("DeathYear");

        // Nationality
        builder.Property(a => a.Nationality)
            .HasMaxLength(100)
            .HasColumnName("Nationality");

        // ExternalIds as owned type
        builder.OwnsOne(a => a.ExternalIds, ext =>
        {
            ext.Property(e => e.GutenbergAuthorId)
                .HasColumnName("GutenbergAuthorId");

            ext.Property(e => e.OpenLibraryAuthorId)
                .HasMaxLength(50)
                .HasColumnName("OpenLibraryAuthorId");

            ext.Property(e => e.WikipediaUrl)
                .HasMaxLength(500)
                .HasColumnName("WikipediaUrl");

            ext.Property(e => e.WikidataId)
                .HasMaxLength(50)
                .HasColumnName("WikidataId");
        });

        // New Indexes
        builder.HasIndex("GutenbergAuthorId")
            .IsUnique()
            .HasFilter("[GutenbergAuthorId] IS NOT NULL")
            .HasDatabaseName("UX_Authors_GutenbergAuthorId");

        builder.HasIndex(a => a.BirthYear)
            .HasDatabaseName("IX_Authors_BirthYear");
    }

    private static HashSet<BookId> DeserializeBookIds(string json)
    {
        if (string.IsNullOrEmpty(json))
            return new HashSet<BookId>();

        var guids = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(json,
            (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>();

        return guids.Select(g => BookId.From(g)).ToHashSet();
    }
}