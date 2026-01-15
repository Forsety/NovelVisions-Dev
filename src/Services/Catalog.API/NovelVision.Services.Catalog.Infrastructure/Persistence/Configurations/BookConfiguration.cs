// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/Persistence/Configurations/BookConfiguration.cs
// ПОЛНАЯ КОНФИГУРАЦИЯ Book Aggregate - минимум игноров
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovelVision.Services.Catalog.Domain.Aggregates.AuthorAggregate;
using NovelVision.Services.Catalog.Domain.Aggregates.BookAggregate;
using NovelVision.Services.Catalog.Domain.Entities;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;
using NovelVision.Services.Catalog.Domain.ValueObjects;

namespace NovelVision.Services.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core конфигурация для Book агрегата
/// </summary>
public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books", "Catalog");

        // ══════════════════════════════════════════════════════════════
        // PRIMARY KEY
        // ══════════════════════════════════════════════════════════════
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .HasColumnName("BookId")
            .HasConversion(
                id => id.Value,
                value => BookId.From(value));

        // ══════════════════════════════════════════════════════════════
        // METADATA (Owned Type)
        // ══════════════════════════════════════════════════════════════
        builder.OwnsOne(b => b.Metadata, metadata =>
        {
            metadata.Property(m => m.Title)
                .HasColumnName("Title")
                .HasMaxLength(500)
                .IsRequired();

            metadata.Property(m => m.Subtitle)
                .HasColumnName("Subtitle")
                .HasMaxLength(500);

            metadata.Property(m => m.Description)
                .HasColumnName("Description")
                .HasMaxLength(10000);

            metadata.Property(m => m.Language)
                .HasColumnName("Language")
                .HasMaxLength(10)
                .IsRequired();

            metadata.Property(m => m.PageCount)
                .HasColumnName("PageCount");

            metadata.Property(m => m.OriginalTitle)
                .HasColumnName("OriginalTitle")
                .HasMaxLength(500);
        });

        // ══════════════════════════════════════════════════════════════
        // AUTHOR ID
        // ══════════════════════════════════════════════════════════════
        builder.Property(b => b.AuthorId)
            .HasColumnName("AuthorId")
            .HasConversion(
                id => id.Value,
                value => AuthorId.From(value))
            .IsRequired();

        // ══════════════════════════════════════════════════════════════
        // ISBN (Owned Type)
        // ══════════════════════════════════════════════════════════════
        builder.OwnsOne(b => b.ISBN, isbn =>
        {
            isbn.Property(i => i.Value)
                .HasColumnName("ISBN")
                .HasMaxLength(20);
        });

        // ══════════════════════════════════════════════════════════════
        // PUBLICATION INFO (Owned Type)
        // ══════════════════════════════════════════════════════════════
        builder.OwnsOne(b => b.PublicationInfo, pubInfo =>
        {
            pubInfo.Property(p => p.Publisher)
                .HasColumnName("Publisher")
                .HasMaxLength(200);

            pubInfo.Property(p => p.PublicationDate)
                .HasColumnName("PublicationDate");

            pubInfo.Property(p => p.Edition)
                .HasColumnName("Edition")
                .HasMaxLength(50);
        });

        // ══════════════════════════════════════════════════════════════
        // STATISTICS (Owned Type)
        // ══════════════════════════════════════════════════════════════
        builder.OwnsOne(b => b.Statistics, stats =>
        {
            stats.Property(s => s.ViewCount)
                .HasColumnName("Stats_ViewCount");

            stats.Property(s => s.DownloadCount)
                .HasColumnName("Stats_DownloadCount");

            stats.Property(s => s.FavoriteCount)
                .HasColumnName("Stats_FavoriteCount");

            stats.Property(s => s.AverageRating)
                .HasColumnName("Stats_AverageRating")
                .HasPrecision(3, 2);

            stats.Property(s => s.RatingCount)
                .HasColumnName("Stats_RatingCount");

            stats.Property(s => s.ReviewCount)
                .HasColumnName("Stats_ReviewCount");

            stats.Property(s => s.CompletedReadCount)
                .HasColumnName("Stats_CompletedReadCount");

            stats.Property(s => s.VisualizationCount)
                .HasColumnName("Stats_VisualizationCount");
        });

        // ══════════════════════════════════════════════════════════════
        // COVER IMAGE (Owned Type)
        // ══════════════════════════════════════════════════════════════
        builder.OwnsOne(b => b.CoverImage, cover =>
        {
            cover.Property(c => c.Url)
                .HasColumnName("CoverImageUrl")
                .HasMaxLength(2000);

            cover.Property(c => c.ThumbnailUrl)
                .HasColumnName("CoverThumbnailUrl")
                .HasMaxLength(2000);

            cover.Property(c => c.AltText)
                .HasColumnName("CoverAltText")
                .HasMaxLength(500);
        });

        // ══════════════════════════════════════════════════════════════
        // STATUS (SmartEnum)
        // ══════════════════════════════════════════════════════════════
        builder.Property(b => b.Status)
            .HasColumnName("Status")
            .HasConversion(
                status => status.Value,
                value => BookStatus.FromValue(value))
            .IsRequired();

        // ══════════════════════════════════════════════════════════════
        // COPYRIGHT STATUS (SmartEnum)
        // ══════════════════════════════════════════════════════════════
        builder.Property(b => b.CopyrightStatus)
            .HasColumnName("CopyrightStatus")
            .HasConversion(
                status => status.Value,
                value => CopyrightStatus.FromValue(value));

        // ══════════════════════════════════════════════════════════════
        // VISUALIZATION MODE (SmartEnum)
        // ══════════════════════════════════════════════════════════════
        builder.Property(b => b.VisualizationMode)
            .HasColumnName("VisualizationModeId")
            .HasConversion(
                mode => mode.Value,
                value => VisualizationMode.FromValue(value));

        // ══════════════════════════════════════════════════════════════
        // VISUALIZATION SETTINGS (Owned Type)
        // ══════════════════════════════════════════════════════════════
        builder.OwnsOne(b => b.VisualizationSettings, vs =>
        {
            vs.Property(s => s.PrimaryMode)
                .HasColumnName("VS_PrimaryMode")
                .HasConversion(
                    mode => mode.Value,
                    value => VisualizationMode.FromValue(value));

            vs.Property(s => s.AllowReaderChoice)
                .HasColumnName("VS_AllowReaderChoice");

            vs.Property(s => s.AllowedModesJson)
                .HasColumnName("VS_AllowedModes")
                .HasMaxLength(100);

            vs.Property(s => s.PreferredStyle)
                .HasColumnName("VS_PreferredStyle")
                .HasMaxLength(50);

            vs.Property(s => s.PreferredProvider)
                .HasColumnName("VS_PreferredProvider")
                .HasMaxLength(50);

            vs.Property(s => s.MaxImagesPerPage)
                .HasColumnName("VS_MaxImagesPerPage");

            vs.Property(s => s.AutoGenerateOnPublish)
                .HasColumnName("VS_AutoGenerateOnPublish");

            vs.Property(s => s.IsEnabled)
                .HasColumnName("VS_IsEnabled");

            // AllowedModes - derived от AllowedModesJson, runtime-only
            vs.Ignore(s => s.AllowedModes);
        });

        // ══════════════════════════════════════════════════════════════
        // EXTERNAL IDS (Owned Type)
        // ══════════════════════════════════════════════════════════════
        builder.OwnsOne(b => b.ExternalIds, ext =>
        {
            ext.Property(e => e.ExternalId)
                .HasColumnName("ExternalId")
                .HasMaxLength(100);

            ext.Property(e => e.SourceType)
                .HasColumnName("ExternalSourceType")
                .HasConversion(
                    st => st.Value,
                    v => ExternalSourceType.FromValue(v));

            ext.Property(e => e.SourceUrl)
                .HasColumnName("ExternalSourceUrl")
                .HasMaxLength(2000);

            ext.Property(e => e.ImportedAt)
                .HasColumnName("ExternalImportedAt");

            ext.Property(e => e.LastSyncedAt)
                .HasColumnName("ExternalLastSyncedAt");

            ext.Property(e => e.GutenbergId)
                .HasColumnName("GutenbergId");

            ext.Property(e => e.OpenLibraryWorkId)
                .HasColumnName("OpenLibraryWorkId")
                .HasMaxLength(50);

            ext.Property(e => e.OpenLibraryEditionId)
                .HasColumnName("OpenLibraryEditionId")
                .HasMaxLength(50);

            // Computed properties - runtime-only
            ext.Ignore(e => e.NeedsSync);
            ext.Ignore(e => e.HasGutenbergId);
            ext.Ignore(e => e.HasOpenLibraryId);
            ext.Ignore(e => e.GutenbergUrl);
            ext.Ignore(e => e.OpenLibraryUrl);
        });

        // ══════════════════════════════════════════════════════════════
        // SOURCE (SmartEnum)
        // ══════════════════════════════════════════════════════════════
        builder.Property(b => b.Source)
            .HasColumnName("Source")
            .HasConversion(
                source => source.Value,
                value => BookSource.FromValue(value));

        // ══════════════════════════════════════════════════════════════
        // SCALAR PROPERTIES
        // ══════════════════════════════════════════════════════════════
        builder.Property(b => b.DownloadCount)
            .HasColumnName("DownloadCount");

        builder.Property(b => b.OriginalPublicationYear)
            .HasColumnName("OriginalPublicationYear");

        builder.Property(b => b.WordCount)
            .HasColumnName("WordCount");

        builder.Property(b => b.ReadingDifficulty)
            .HasColumnName("ReadingDifficulty")
            .HasPrecision(5, 2);

        builder.Property(b => b.FullTextUrl)
            .HasColumnName("FullTextUrl")
            .HasMaxLength(2000);

        builder.Property(b => b.HasFullText)
            .HasColumnName("HasFullText");

        // ══════════════════════════════════════════════════════════════
        // GENRES (backing field as comma-separated)
        // ══════════════════════════════════════════════════════════════
        builder.Property<HashSet<string>>("_genres")
            .HasColumnName("Genres")
            .HasConversion(
                genres => string.Join(",", genres),
                str => string.IsNullOrEmpty(str)
                    ? new HashSet<string>()
                    : str.Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet(),
                new ValueComparer<HashSet<string>>(
                    (c1, c2) => c1!.SetEquals(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToHashSet()))
            .HasMaxLength(2000);

        // ══════════════════════════════════════════════════════════════
        // TAGS (backing field as comma-separated)
        // ══════════════════════════════════════════════════════════════
        builder.Property<HashSet<string>>("_tags")
            .HasColumnName("Tags")
            .HasConversion(
                tags => string.Join(",", tags),
                str => string.IsNullOrEmpty(str)
                    ? new HashSet<string>()
                    : str.Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet(),
                new ValueComparer<HashSet<string>>(
                    (c1, c2) => c1!.SetEquals(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToHashSet()))
            .HasMaxLength(2000);

        // ══════════════════════════════════════════════════════════════
        // BASE ENTITY PROPERTIES
        // ══════════════════════════════════════════════════════════════
        builder.Property(b => b.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .IsRequired();

        builder.Property(b => b.Version)
            .HasColumnName("Version")
            .IsConcurrencyToken();

        // ══════════════════════════════════════════════════════════════
        // RELATIONSHIPS
        // ══════════════════════════════════════════════════════════════

        // Chapters (One-to-Many)
        builder.HasMany(b => b.Chapters)
            .WithOne()
            .HasForeignKey(c => c.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        // Author (Many-to-One)
        builder.HasOne<Author>()
            .WithMany()
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // BookSubjects (Many-to-Many через BookSubject entity)
        // Настраивается в BookSubjectConfiguration

        // ══════════════════════════════════════════════════════════════
        // INDEXES
        // ══════════════════════════════════════════════════════════════
        builder.HasIndex(b => b.AuthorId)
            .HasDatabaseName("IX_Books_AuthorId");

        builder.HasIndex(b => b.Status)
            .HasDatabaseName("IX_Books_Status");

        builder.HasIndex(b => b.Source)
            .HasDatabaseName("IX_Books_Source");

        builder.HasIndex(b => b.CreatedAt)
            .HasDatabaseName("IX_Books_CreatedAt");

        builder.HasIndex(b => b.CopyrightStatus)
            .HasDatabaseName("IX_Books_CopyrightStatus");

        // ══════════════════════════════════════════════════════════════
        // IGNORE - только transient/runtime-only свойства
        // ══════════════════════════════════════════════════════════════

        // DomainEvents - transient collection
        builder.Ignore(b => b.DomainEvents);

        // ExternalId - obsolete alias для ExternalIds
#pragma warning disable CS0618
        builder.Ignore(b => b.ExternalId);
#pragma warning restore CS0618

        // ChapterCount, TotalPageCount, TotalWordCount - вычисляются из navigation
        // Это реальные computed properties без backing field
        builder.Ignore(b => b.ChapterCount);
        builder.Ignore(b => b.TotalPageCount);
        builder.Ignore(b => b.TotalWordCount);

        // SubjectIds - runtime-only HashSet
        builder.Ignore(b => b.SubjectIds);

        // BookSubjects - navigation property, настраивается отдельно
        builder.Ignore(b => b.BookSubjects);

        // Genres, Tags - readonly свойства от backing fields
        // Backing fields (_genres, _tags) уже маппятся
        builder.Ignore(b => b.Genres);
        builder.Ignore(b => b.Tags);
    }
}