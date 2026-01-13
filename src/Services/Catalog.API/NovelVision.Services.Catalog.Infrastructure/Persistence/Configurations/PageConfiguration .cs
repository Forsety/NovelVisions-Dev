// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/Persistence/Configurations/PageConfiguration.cs
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovelVision.Services.Catalog.Domain.Entities;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core конфигурация для Page сущности
/// </summary>
public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.ToTable("Pages", "Catalog");

        // Primary Key
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("PageId")
            .HasConversion(
                id => id.Value,
                value => PageId.From(value));

        // ChapterId
        builder.Property(p => p.ChapterId)
            .HasColumnName("ChapterId")
            .HasConversion(
                id => id.Value,
                value => ChapterId.From(value))
            .IsRequired();

        // PageNumber
        builder.Property(p => p.PageNumber)
            .HasColumnName("PageNumber")
            .IsRequired();

        // Content
        builder.Property<string>("_content")
            .HasColumnName("Content")
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        // Visualization Properties
        builder.Property(p => p.VisualizationImageUrl)
            .HasColumnName("VisualizationImageUrl")
            .HasMaxLength(2000);

        builder.Property(p => p.VisualizationThumbnailUrl)
            .HasColumnName("VisualizationThumbnailUrl")
            .HasMaxLength(2000);

        builder.Property(p => p.VisualizationJobId)
            .HasColumnName("VisualizationJobId");

        builder.Property(p => p.IsVisualizationPoint)
            .HasColumnName("IsVisualizationPoint")
            .HasDefaultValue(false);

        builder.Property(p => p.AuthorVisualizationHint)
            .HasColumnName("AuthorVisualizationHint")
            .HasMaxLength(1000);

        builder.Property(p => p.VisualizationGeneratedAt)
            .HasColumnName("VisualizationGeneratedAt");

        builder.Property(p => p.VisualizationStatus)
            .HasColumnName("VisualizationStatus")
            .HasConversion<int>()
            .HasDefaultValue(VisualizationStatus.None);

        // Legacy VisualizationPrompts (JSON array)
        builder.Property<List<string>>("_visualizationPrompts")
            .HasColumnName("VisualizationPrompts")
            .HasConversion(
                prompts => string.Join("|||", prompts),
                str => string.IsNullOrEmpty(str)
                    ? new List<string>()
                    : str.Split("|||", System.StringSplitOptions.RemoveEmptyEntries).ToList(),
                new ValueComparer<List<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()))
            .HasMaxLength(4000);

        // Base Entity Properties
        builder.Property(p => p.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .IsRequired();

        // Indexes
        builder.HasIndex(p => p.ChapterId)
            .HasDatabaseName("IX_Pages_ChapterId");

        builder.HasIndex(p => new { p.ChapterId, p.PageNumber })
            .IsUnique()
            .HasDatabaseName("IX_Pages_ChapterId_PageNumber");

        builder.HasIndex(p => p.IsVisualizationPoint)
            .HasDatabaseName("IX_Pages_IsVisualizationPoint")
            .HasFilter("[IsVisualizationPoint] = 1");

        builder.HasIndex(p => p.VisualizationStatus)
            .HasDatabaseName("IX_Pages_VisualizationStatus");

        // Ignore computed properties
        builder.Ignore(p => p.WordCount);
        builder.Ignore(p => p.CharacterCount);
        builder.Ignore(p => p.EstimatedReadingTime);
        builder.Ignore(p => p.HasVisualization);
        builder.Ignore(p => p.IsPendingVisualization);
        builder.Ignore(p => p.Content);
        builder.Ignore(p => p.VisualizationPrompts);
        builder.Ignore(p => p.DomainEvents);
    }
}