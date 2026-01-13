// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/Persistence/Configurations/BookConfiguration.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

        // Primary Key
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .HasColumnName("BookId")
            .HasConversion(
                id => id.Value,
                value => BookId.From(value));

        // Metadata (Owned Type)
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

        // AuthorId
        builder.Property(b => b.AuthorId)
            .HasColumnName("AuthorId")
            .HasConversion(
                id => id.Value,
                value => AuthorId.From(value))
            .IsRequired();

        // ISBN (Owned Type)
        builder.OwnsOne(b => b.ISBN, isbn =>
        {
            isbn.Property(i => i.Value)
                .HasColumnName("ISBN")
                .HasMaxLength(20);
        });

        // PublicationInfo (Owned Type)
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

        // Status (SmartEnum)
        builder.Property(b => b.Status)
            .HasColumnName("Status")
            .HasConversion(
                status => status.Value,
                value => BookStatus.FromValue(value))
            .IsRequired();

        // VisualizationMode (SmartEnum)
        builder.Property(b => b.VisualizationMode)
            .HasColumnName("VisualizationModeId")
            .HasConversion(
                mode => mode.Value,
                value => VisualizationMode.FromValue(value));

        // VisualizationSettings (Owned Type)
        builder.OwnsOne(b => b.VisualizationSettings, vs =>
        {
            vs.Property(s => s.PrimaryMode)
                .HasColumnName("VS_PrimaryMode")
                .HasConversion(
                    mode => mode.Value,
                    value => VisualizationMode.FromValue(value));

            vs.Property(s => s.AllowReaderChoice)
                .HasColumnName("VS_AllowReaderChoice");

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

            // AllowedModes как JSON
            vs.Property<List<int>>("_allowedModes")
                .HasColumnName("VS_AllowedModes")
                .HasConversion(
                    modes => string.Join(",", modes),
                    str => string.IsNullOrEmpty(str)
                        ? new List<int>()
                        : str.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(int.Parse)
                            .ToList(),
                    new ValueComparer<List<int>>(
                        (c1, c2) => c1!.SequenceEqual(c2!),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));
        });

        // CoverImage (Owned Type)
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

        // Statistics (Owned Type)
        builder.OwnsOne(b => b.Statistics, stats =>
        {
            stats.Property(s => s.ViewCount)
                .HasColumnName("ViewCount");

            stats.Property(s => s.DownloadCount)
                .HasColumnName("DownloadCount");

            stats.Property(s => s.FavoriteCount)
                .HasColumnName("FavoriteCount");

            stats.Property(s => s.AverageRating)
                .HasColumnName("AverageRating")
                .HasPrecision(3, 2);

            stats.Property(s => s.RatingCount)
                .HasColumnName("RatingCount");
        });

        // ExternalId (Owned Type)
        builder.OwnsOne(b => b.ExternalId, extId =>
        {
            extId.Property(e => e.ExternalId)
                .HasColumnName("ExternalId")
                .HasMaxLength(100);

            extId.Property(e => e.SourceType)
                .HasColumnName("ExternalSourceType")
                .HasConversion<int>();

            extId.Property(e => e.SourceUrl)
                .HasColumnName("ExternalSourceUrl")
                .HasMaxLength(2000);

            extId.Property(e => e.LastSyncedAt)
                .HasColumnName("ExternalLastSyncedAt");
        });

        // CopyrightStatus
        builder.Property(b => b.CopyrightStatus)
            .HasColumnName("CopyrightStatus")
            .HasConversion(
                status => status.Value,
                value => CopyrightStatus.FromValue(value));

        // Scalar Properties
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

        // Genres (JSON array)
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
            .HasMaxLength(1000);

        // Tags (JSON array)
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

        // Chapters Navigation
        builder.HasMany(b => b.Chapters)
            .WithOne()
            .HasForeignKey(c => c.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        // Subjects Navigation
        builder.HasMany(b => b.BookSubjects)
            .WithMany()
            .UsingEntity(j => j.ToTable("BookSubjects", "Catalog"));

        // Base Entity Properties
        builder.Property(b => b.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .IsRequired();

        builder.Property(b => b.Version)
            .HasColumnName("Version")
            .IsConcurrencyToken();

        // Indexes
        builder.HasIndex(b => b.AuthorId)
            .HasDatabaseName("IX_Books_AuthorId");

        builder.HasIndex(b => b.Status)
            .HasDatabaseName("IX_Books_Status");

        builder.HasIndex(b => b.CreatedAt)
            .HasDatabaseName("IX_Books_CreatedAt");

        builder.HasIndex("_genres")
            .HasDatabaseName("IX_Books_Genres");

        // Ignore domain events
        builder.Ignore(b => b.DomainEvents);
    }
}