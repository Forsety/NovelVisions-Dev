// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/Persistence/Configurations/SubjectConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovelVision.Services.Catalog.Domain.Entities;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Infrastructure.Persistence.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("Subjects", "Catalog");

        // Primary Key
        builder.HasKey(s => s.Id);

        // StronglyTypedId conversion
        builder.Property(s => s.Id)
            .HasConversion(
                v => v.Value,
                v => SubjectId.From(v))
            .ValueGeneratedNever()
            .HasColumnName("SubjectId");

        // Properties
        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired()
            .HasColumnName("Name");

        builder.Property(s => s.Type)
            .HasConversion<int>()
            .IsRequired()
            .HasColumnName("SubjectType");

        builder.Property(s => s.Description)
            .HasMaxLength(1000)
            .HasColumnName("Description");

        builder.Property(s => s.Slug)
            .HasMaxLength(200)
            .IsRequired()
            .HasColumnName("Slug");

        builder.Property(s => s.ExternalMapping)
            .HasMaxLength(500)
            .HasColumnName("ExternalMapping");

        // ParentId - self-referencing relationship
        builder.Property(s => s.ParentId)
            .HasConversion(
                v => v != null ? v.Value : (Guid?)null,
                v => v.HasValue ? SubjectId.From(v.Value) : null)
            .HasColumnName("ParentSubjectId");

        // Timestamps
        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasColumnName("CreatedAt");

        builder.Property(s => s.UpdatedAt)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasColumnName("UpdatedAt");

        // Self-referencing relationship
        builder.HasOne<Subject>()
            .WithMany()
            .HasForeignKey(s => s.ParentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(s => s.Name)
            .HasDatabaseName("IX_Subjects_Name");

        builder.HasIndex(s => s.Slug)
            .IsUnique()
            .HasDatabaseName("UX_Subjects_Slug");

        builder.HasIndex(s => s.Type)
            .HasDatabaseName("IX_Subjects_Type");

        builder.HasIndex(s => s.ParentId)
            .HasDatabaseName("IX_Subjects_ParentId");

        // Ignore computed properties
        builder.Ignore(s => s.DomainEvents);
    }
}