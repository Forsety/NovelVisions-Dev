using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovelVision.Services.Catalog.Domain.Entities;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Infrastructure.Persistence.Configurations;

public class ChapterConfiguration : IEntityTypeConfiguration<Chapter>
{
    public void Configure(EntityTypeBuilder<Chapter> builder)
    {
        builder.ToTable("Chapters", "Catalog");

        // Primary Key
        builder.HasKey(c => c.Id);

        // StronglyTypedId conversion
        builder.Property(c => c.Id)
            .HasConversion(
                v => v.Value,
                v => ChapterId.From(v))
            .ValueGeneratedNever()
            .HasColumnName("ChapterId");

        // BookId conversion
        builder.Property(c => c.BookId)
            .HasConversion(
                v => v.Value,
                v => BookId.From(v))
            .IsRequired()
            .HasColumnName("BookId");

        // Properties
        builder.Property(c => c.Title)
            .HasMaxLength(200)
            .IsRequired()
            .HasColumnName("Title");

        builder.Property(c => c.Summary)
            .HasMaxLength(500)
            .HasColumnName("Summary");

        builder.Property(c => c.OrderIndex)
            .IsRequired()
            .HasColumnName("OrderIndex");

        // Timestamps
        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasColumnName("CreatedAt");

        builder.Property(c => c.UpdatedAt)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasColumnName("UpdatedAt");

        // Relationships
        builder.HasMany(c => c.Pages)
            .WithOne()
            .HasForeignKey(p => p.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.BookId)
            .HasDatabaseName("IX_Chapters_BookId");

        builder.HasIndex(c => new { c.BookId, c.OrderIndex })
            .IsUnique()
            .HasDatabaseName("IX_Chapters_BookId_OrderIndex");

        // Ignore computed properties
        builder.Ignore(c => c.PageCount);
        builder.Ignore(c => c.TotalWordCount);
        builder.Ignore(c => c.EstimatedReadingTime);
        builder.Ignore(c => c.DomainEvents);
    }
}
