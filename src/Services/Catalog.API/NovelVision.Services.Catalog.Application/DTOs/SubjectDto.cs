// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/DTOs/SubjectDto.cs
using System;
using System.Collections.Generic;

namespace NovelVision.Services.Catalog.Application.DTOs;

/// <summary>
/// DTO для категории/темы
/// </summary>
public record SubjectDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string TypeDescription { get; init; } = string.Empty;
    public Guid? ParentId { get; init; }
    public string? ParentName { get; init; }
    public string? Description { get; init; }
    public string Slug { get; init; } = string.Empty;
    public int BookCount { get; init; }
    public string? ExternalMapping { get; init; }
    public bool IsRoot { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// DTO для списка категорий (краткая версия)
/// </summary>
public record SubjectListDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public int BookCount { get; init; }
    public bool HasChildren { get; init; }
}

/// <summary>
/// DTO для иерархии категорий
/// </summary>
public record SubjectHierarchyDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public int BookCount { get; init; }
    public List<SubjectHierarchyDto> Children { get; init; } = new();
}

/// <summary>
/// DTO для создания категории
/// </summary>
public record CreateSubjectDto
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public Guid? ParentId { get; init; }
    public string? Description { get; init; }
    public string? ExternalMapping { get; init; }
}