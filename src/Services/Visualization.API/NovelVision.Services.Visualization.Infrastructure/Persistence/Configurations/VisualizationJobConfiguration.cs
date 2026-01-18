// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Persistence/Configurations/VisualizationJobConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NovelVision.Services.Visualization.Domain.Aggregates.VisualizationJobAggregate;
using NovelVision.Services.Visualization.Domain.Enums;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;
using NovelVision.Services.Visualization.Domain.ValueObjects;

namespace NovelVision.Services.Visualization.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core конфигурация для VisualizationJob
/// </summary>
public sealed class VisualizationJobConfiguration : IEntityTypeConfiguration<VisualizationJob>
{
    public void Configure(EntityTypeBuilder<VisualizationJob> builder)
    {
        // ══════════════════════════════════════════════════════════════
        // TABLE
        // ══════════════════════════════════════════════════════════════
        builder.ToTable("VisualizationJobs", "visualization");

        // ══════════════════════════════════════════════════════════════
        // PRIMARY KEY (Strongly-typed ID)
        // ══════════════════════════════════════════════════════════════
        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .HasConversion(
                id => id.Value,
                value => VisualizationJobId.From(value))
            .HasColumnName("Id")
            .IsRequired();

        // ══════════════════════════════════════════════════════════════
        // FOREIGN KEYS (External IDs - not FK constraints)
        // ══════════════════════════════════════════════════════════════
        builder.Property(j => j.BookId)
            .HasColumnName("BookId")
            .IsRequired();

        builder.Property(j => j.PageId)
            .HasColumnName("PageId");

        builder.Property(j => j.ChapterId)
            .HasColumnName("ChapterId");

        builder.Property(j => j.UserId)
            .HasColumnName("UserId")
            .IsRequired();

        // ══════════════════════════════════════════════════════════════
        // SMART ENUMS
        // ══════════════════════════════════════════════════════════════
        builder.Property(j => j.Trigger)
            .HasConversion(
                trigger => trigger.Value,
                value => VisualizationTrigger.FromValue(value))
            .HasColumnName("Trigger")
            .IsRequired();

        builder.Property<int>("_status")
    .HasConversion(
        new ValueConverter<int, int>(
            v => v,  
            v => v)) 
    .HasColumnName("Status")
    .IsRequired();

        builder.Property(j => j.PreferredProvider)
            .HasConversion(
                provider => provider.Value,
                value => AIModelProvider.FromValue(value))
            .HasColumnName("PreferredProvider")
            .IsRequired();

        // ══════════════════════════════════════════════════════════════
        // VALUE OBJECTS - GenerationParameters (Owned)
        // ══════════════════════════════════════════════════════════════
        builder.OwnsOne(j => j.Parameters, parametersBuilder =>
        {
            parametersBuilder.Property(p => p.Size)
                .HasColumnName("Parameters_Size")
                .HasMaxLength(50)
                .IsRequired();

            parametersBuilder.Property(p => p.Quality)
                .HasColumnName("Parameters_Quality")
                .HasMaxLength(50)
                .IsRequired();

            parametersBuilder.Property(p => p.AspectRatio)
                .HasColumnName("Parameters_AspectRatio")
                .HasMaxLength(20);

            parametersBuilder.Property(p => p.Seed)
                .HasColumnName("Parameters_Seed");

            parametersBuilder.Property(p => p.Steps)
                .HasColumnName("Parameters_Steps");

            parametersBuilder.Property(p => p.CfgScale)
                .HasColumnName("Parameters_CfgScale");

            parametersBuilder.Property(p => p.Sampler)
                .HasColumnName("Parameters_Sampler")
                .HasMaxLength(100);

            parametersBuilder.Property(p => p.Upscale)
                .HasColumnName("Parameters_Upscale")
                .IsRequired();
        });

        // ══════════════════════════════════════════════════════════════
        // VALUE OBJECTS - TextSelection (Owned, Optional)
        // ══════════════════════════════════════════════════════════════
        builder.OwnsOne(j => j.TextSelection, textBuilder =>
        {
            textBuilder.Property(t => t.SelectedText)
                .HasColumnName("TextSelection_SelectedText")
                .HasMaxLength(5000);

            textBuilder.Property(t => t.StartPosition)
                .HasColumnName("TextSelection_StartPosition");

            textBuilder.Property(t => t.EndPosition)
                .HasColumnName("TextSelection_EndPosition");

            textBuilder.Property(t => t.PageId)
                .HasColumnName("TextSelection_PageId");

            textBuilder.Property(t => t.ChapterId)
                .HasColumnName("TextSelection_ChapterId");

            textBuilder.Property(t => t.ContextBefore)
                .HasColumnName("TextSelection_ContextBefore")
                .HasMaxLength(500);

            textBuilder.Property(t => t.ContextAfter)
                .HasColumnName("TextSelection_ContextAfter")
                .HasMaxLength(500);
        });

        // ══════════════════════════════════════════════════════════════
        // VALUE OBJECTS - PromptData (Owned, Optional)
        // ══════════════════════════════════════════════════════════════
        builder.OwnsOne<PromptData>("_promptData", promptBuilder =>
        {
            promptBuilder.Property(p => p.OriginalText)
                .HasColumnName("PromptData_OriginalText")
                .HasMaxLength(10000);

            promptBuilder.Property(p => p.EnhancedPrompt)
                .HasColumnName("PromptData_EnhancedPrompt")
                .HasMaxLength(10000);

            promptBuilder.Property(p => p.NegativePrompt)
                .HasColumnName("PromptData_NegativePrompt")
                .HasMaxLength(5000);

            promptBuilder.Property(p => p.TargetModel)
                .HasConversion(
                    model => model.Value,
                    value => AIModelProvider.FromValue(value))
                .HasColumnName("PromptData_TargetModel");

            promptBuilder.Property(p => p.Style)
                .HasColumnName("PromptData_Style")
                .HasMaxLength(100);

            // Parameters dictionary as JSON
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
        // SCALAR PROPERTIES
        // ══════════════════════════════════════════════════════════════
        builder.Property(j => j.Priority)
            .HasColumnName("Priority")
            .IsRequired();

        builder.Property("_errorMessage")
            .HasColumnName("ErrorMessage")
            .HasMaxLength(2000);

        builder.Property("_retryCount")
            .HasColumnName("RetryCount")
            .IsRequired();

        builder.Property(j => j.ProcessingStartedAt)
            .HasColumnName("ProcessingStartedAt");

        builder.Property(j => j.CompletedAt)
            .HasColumnName("CompletedAt");

        builder.Property(j => j.ExternalJobId)
            .HasColumnName("ExternalJobId")
            .HasMaxLength(500);

        // ══════════════════════════════════════════════════════════════
        // BASE ENTITY PROPERTIES
        // ══════════════════════════════════════════════════════════════
        builder.Property(j => j.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(j => j.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .IsRequired();

        builder.Property(j => j.Version)
            .HasColumnName("Version")
            .IsConcurrencyToken()
            .IsRequired();

        // ══════════════════════════════════════════════════════════════
        // RELATIONSHIPS
        // ══════════════════════════════════════════════════════════════
        builder.HasMany(j => j.Images)
            .WithOne()
            .HasForeignKey(i => i.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation property backing field
        builder.Navigation(j => j.Images)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ══════════════════════════════════════════════════════════════
        // INDEXES
        // ══════════════════════════════════════════════════════════════
        builder.HasIndex(j => j.BookId)
            .HasDatabaseName("IX_VisualizationJobs_BookId");

        builder.HasIndex(j => j.PageId)
            .HasDatabaseName("IX_VisualizationJobs_PageId")
            .HasFilter("[PageId] IS NOT NULL");

        builder.HasIndex(j => j.UserId)
            .HasDatabaseName("IX_VisualizationJobs_UserId");

        builder.HasIndex("_status")
            .HasDatabaseName("IX_VisualizationJobs_Status");

        builder.HasIndex(j => j.CreatedAt)
            .HasDatabaseName("IX_VisualizationJobs_CreatedAt")
            .IsDescending();

        // Composite index for queue processing
        builder.HasIndex("_status", "Priority", "CreatedAt")
            .HasDatabaseName("IX_VisualizationJobs_Queue")
            .IsDescending(false, true, false);

        // ══════════════════════════════════════════════════════════════
        // IGNORE COMPUTED PROPERTIES
        // ══════════════════════════════════════════════════════════════
        builder.Ignore(j => j.HasImages);
        builder.Ignore(j => j.SelectedImage);
        builder.Ignore(j => j.ProcessingTime);
        builder.Ignore(j => j.CanCancel);
        builder.Ignore(j => j.CanRetry);
        builder.Ignore(j => j.Status);
        builder.Ignore(j => j.PromptData);
        builder.Ignore(j => j.ErrorMessage);
        builder.Ignore(j => j.RetryCount);
        builder.Ignore(j => j.DomainEvents);
    }
}