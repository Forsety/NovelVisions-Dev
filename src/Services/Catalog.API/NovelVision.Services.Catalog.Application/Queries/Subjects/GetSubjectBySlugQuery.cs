// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Queries/Subjects/GetSubjectBySlugQuery.cs
using System;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Queries.Subjects;

/// <summary>
/// Запрос категории по slug
/// </summary>
public record GetSubjectBySlugQuery : IRequest<Result<SubjectDto>>
{
    /// <summary>
    /// Slug категории для поиска
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Включать ли дочерние категории
    /// </summary>
    public bool IncludeChildren { get; init; } = false;

    /// <summary>
    /// Включать ли книги категории
    /// </summary>
    public bool IncludeBooks { get; init; } = false;

    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public GetSubjectBySlugQuery() { }

    /// <summary>
    /// Конструктор с slug
    /// </summary>
    public GetSubjectBySlugQuery(string slug)
    {
        Slug = slug;
    }

    /// <summary>
    /// Конструктор с slug и опциями
    /// </summary>
    public GetSubjectBySlugQuery(string slug, bool includeChildren = false, bool includeBooks = false)
    {
        Slug = slug;
        IncludeChildren = includeChildren;
        IncludeBooks = includeBooks;
    }
}