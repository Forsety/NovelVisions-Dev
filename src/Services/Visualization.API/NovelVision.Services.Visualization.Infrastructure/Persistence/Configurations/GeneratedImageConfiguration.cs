// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Persistence/Configurations/GeneratedImageConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovelVision.Services.Visualization.Domain.Aggregates.VisualizationJobAggregate;
using NovelVision.Services.Visualization.Domain.Enums;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;
using NovelVision.Services.Visualization.Domain.ValueObjects;

namespace NovelVision.Services.Visualization.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core конфигурация для GeneratedImage
/// </summary>
public sealed class GeneratedImageConfiguration : IEntityTypeConfiguration<GeneratedImage>
{
    public void Configure(EntityTypeBuilder<GeneratedImage> builder)
    {
        // ══════════════════════════════════════════════════════════════
        // TABLE
        // ══════════════════════════════════════════════════════════════
        builder.ToTable("GeneratedImages", "visualization");

        // ══════════════════════════════════════════════════════════════
        // PRIMARY KEY (Strongly-typed ID)
        // ══════════════════════════════════════════════════════════════
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasConversion(
                id => id.Value,
                value => GeneratedImageId.From(value))
            .HasColumnName("Id")
            .IsRequired();

        // ══════════════════════════════════════════════════════════════
        // FOREIGN KEY
        // ══════════════════════════════════════════════════════════════
        builder.Property(i => i.JobId)
            .HasConversion(
                id => id.Value,
                value => VisualizationJobId.From(value))
            .HasColumnName("JobId")
            .IsRequired();

        // ══════════════════════════════════════════════════════════════
        // VALUE OBJECTS - ImageMetadata (Owned)
        // ══════════════════════════════════════════════════════════════
        builder.OwnsOne(i => i.Metadata, metadataBuilder =>
        {
            metadataBuilder.Property(m => m.Url)
                .HasColumnName("ImageUrl")
                .HasMaxLength(2000)
                .IsRequired();

            metadataBuilder.Property(m => m.ThumbnailUrl)
                .HasColumnName("ThumbnailUrl")
                .HasMaxLength(2000);

            metadataBuilder.Property(m => m.Width)
                .HasColumnName("Width")
                .IsRequired();

            metadataBuilder.Property(m => m.Height)
                .HasColumnName("Height")
                .IsRequired();

            metadataBuilder.Property(m => m.FileSizeBytes)
                .HasColumnName("FileSizeBytes")
                .IsRequired();

            metadataBuilder.Property(m => m.Format)
                .HasConversion(
                    format => format.Value,
                    value => ImageFormat.FromValue(value))
                .HasColumnName("Format")
                .IsRequired();

            metadataBuilder.Property(m => m.BlobPath)
                .HasColumnName("BlobPath")
                .HasMaxLength(1000);
        });

        // ══════════════════════════════════════════════════════════════
        // VALUE OBJECTS - PromptData (Owned)
        // ══════════════════════════════════════════════════════════════
        builder.OwnsOne(i => i.PromptData, promptBuilder =>
        {
            promptBuilder.Property(p => p.OriginalText)
                .HasColumnName("PromptData_OriginalText")
                .HasMaxLength(10000)
                .IsRequired();

            promptBuilder.Property(p => p.EnhancedPrompt)
                .HasColumnName("PromptData_EnhancedPrompt")
                .HasMaxLength(10000)
                .IsRequired();

            promptBuilder.Property(p => p.NegativePrompt)
                .HasColumnName("PromptData_NegativePrompt")
                .HasMaxLength(5000);

            promptBuilder.Property(p => p.TargetModel)
                .HasConversion(
                    model => model.Value,
                    value => AIModelProvider.FromValue(value))
                .HasColumnName("PromptData_TargetModel")
                .IsRequired();

            promptBuilder.Property(p => p.Style)
                .HasColumnName("PromptData_Style")
                .HasMaxLength(100);

            promptBuilder.Property(p => p.Parameters)
                .HasConversion(
                    dict => System.Text.Json.JsonSerializer.Serialize(dict, (System.Text.Json.JsonSerializerOptions?)null),
                    json => string.IsNullOrEmpty(json)
                        ? new Dictionary<string, object>()
                        : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json, (System.Text.Json.JsonSerializerOptions?)null) ?? new())
                .HasColumnName("PromptData_Parameters")
                .HasMaxLength(5000);
        });

        // ══════════════════════════════════════════════════════════════
        // SMART ENUMS
        // ══════════════════════════════════════════════════════════════
        builder.Property(i => i.Provider)
            .HasConversion(
                provider => provider.Value,
                value => AIModelProvider.FromValue(value))
            .HasColumnName("Provider")
            .IsRequired();

        // ══════════════════════════════════════════════════════════════
        // SCALAR PROPERTIES
        // ══════════════════════════════════════════════════════════════
        builder.Property(i => i.ExternalJobId)
            .HasColumnName("ExternalJobId")
            .HasMaxLength(500);

        builder.Property(i => i.GeneratedAt)
            .HasColumnName("GeneratedAt")
            .IsRequired();

        builder.Property(i => i.IsSelected)
            .HasColumnName("IsSelected")
            .IsRequired();

        builder.Property(i => i.IsDeleted)
            .HasColumnName("IsDeleted")
            .IsRequired();

        // ══════════════════════════════════════════════════════════════
        // BASE ENTITY PROPERTIES
        // ══════════════════════════════════════════════════════════════
        builder.Property(i => i.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .IsRequired();

        // ══════════════════════════════════════════════════════════════
        // INDEXES
        // ══════════════════════════════════════════════════════════════
        builder.HasIndex(i => i.JobId)
            .HasDatabaseName("IX_GeneratedImages_JobId");

        builder.HasIndex(i => i.IsSelected)
            .HasDatabaseName("IX_GeneratedImages_IsSelected")
            .HasFilter("[IsSelected] = 1 AND [IsDeleted] = 0");

        builder.HasIndex(i => i.GeneratedAt)
            .HasDatabaseName("IX_GeneratedImages_GeneratedAt")
            .IsDescending();

        // ══════════════════════════════════════════════════════════════
        // IGNORE
        // ══════════════════════════════════════════════════════════════
        builder.Ignore(i => i.ImageUrl);
        builder.Ignore(i => i.DomainEvents);
    }
}