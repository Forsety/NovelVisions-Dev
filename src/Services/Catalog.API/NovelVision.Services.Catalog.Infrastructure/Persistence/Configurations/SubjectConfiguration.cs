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

        // ═══════════════════════════════════════════════════════════════
        // PRIMARY KEY
        // ═══════════════════════════════════════════════════════════════
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(
                v => v.Value,
                v => SubjectId.From(v))
            .ValueGeneratedNever()
            .HasColumnName("SubjectId");

        // ═══════════════════════════════════════════════════════════════
        // PROPERTIES - все маппятся в БД
        // ═══════════════════════════════════════════════════════════════

        // Name - публичное свойство с private setter
        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired()
            .HasColumnName("Name");

        // Type - SmartEnum с явным конвертером
        builder.Property(s => s.Type)
            .HasConversion(
                type => type.Value,
                value => SubjectType.FromValue(value))
            .IsRequired()
            .HasColumnName("SubjectType");

        // Description - nullable string
        builder.Property(s => s.Description)
            .HasMaxLength(1000)
            .HasColumnName("Description");

        // Slug - теперь хранится в БД (не computed)
        builder.Property(s => s.Slug)
            .HasMaxLength(200)
            .IsRequired()
            .HasColumnName("Slug");

        // ExternalMapping - для связи с внешними источниками
        builder.Property(s => s.ExternalMapping)
            .HasMaxLength(500)
            .HasColumnName("ExternalMapping");

        // BookCount - denormalized counter для производительности
        builder.Property(s => s.BookCount)
            .HasColumnName("BookCount")
            .HasDefaultValue(0);

        // ═══════════════════════════════════════════════════════════════
        // PARENT ID - self-referencing relationship
        // ═══════════════════════════════════════════════════════════════
        builder.Property(s => s.ParentId)
            .HasConversion(
                v => v != null ? v.Value : (Guid?)null,
                v => v.HasValue ? SubjectId.From(v.Value) : null)
            .HasColumnName("ParentSubjectId");

        // ═══════════════════════════════════════════════════════════════
        // BASE ENTITY PROPERTIES
        // ═══════════════════════════════════════════════════════════════
        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasColumnName("CreatedAt");

        builder.Property(s => s.UpdatedAt)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasColumnName("UpdatedAt");

        // ═══════════════════════════════════════════════════════════════
        // RELATIONSHIPS
        // ═══════════════════════════════════════════════════════════════

        // Self-referencing hierarchy
        builder.HasOne<Subject>()
            .WithMany()
            .HasForeignKey(s => s.ParentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // ═══════════════════════════════════════════════════════════════
        // INDEXES
        // ═══════════════════════════════════════════════════════════════
        builder.HasIndex(s => s.Name)
            .HasDatabaseName("IX_Subjects_Name");

        builder.HasIndex(s => s.Slug)
            .IsUnique()
            .HasDatabaseName("UX_Subjects_Slug");

        builder.HasIndex(s => s.Type)
            .HasDatabaseName("IX_Subjects_Type");

        builder.HasIndex(s => s.ParentId)
            .HasDatabaseName("IX_Subjects_ParentId");

        builder.HasIndex(s => s.ExternalMapping)
            .HasDatabaseName("IX_Subjects_ExternalMapping");

        // ═══════════════════════════════════════════════════════════════
        // IGNORE - только runtime-only свойства
        // ═══════════════════════════════════════════════════════════════

        // DomainEvents - transient, не хранится в БД
        builder.Ignore(s => s.DomainEvents);

        // BookIds - runtime-only HashSet, relationship через BookSubjects
        builder.Ignore(s => s.BookIds);

        // IsRoot - простое вычисление от ParentId, не нужно хранить
        builder.Ignore(s => s.IsRoot);
    }
}