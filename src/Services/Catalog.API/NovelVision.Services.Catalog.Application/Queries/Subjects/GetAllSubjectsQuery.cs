// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Queries/Subjects/GetAllSubjectsQuery.cs
using System.Collections.Generic;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Queries.Subjects;

/// <summary>
/// Запрос всех категорий
/// </summary>
public record GetAllSubjectsQuery : IRequest<Result<List<SubjectListDto>>>
{
    public string? Type { get; init; }
    public bool OnlyRoot { get; init; } = false;
    public bool IncludeEmpty { get; init; } = false;
}

/// <summary>
/// Запрос иерархии категорий
/// </summary>
public record GetSubjectHierarchyQuery : IRequest<Result<List<SubjectHierarchyDto>>>
{
    public string? Type { get; init; }
}

/// <summary>
/// Запрос категории по ID
/// </summary>
public record GetSubjectByIdQuery : IRequest<Result<SubjectDto>>
{
    public Guid Id { get; init; }
}
