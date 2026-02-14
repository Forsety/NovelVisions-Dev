// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Queries/Pages/GetPagesForVisualizationQuery.cs
// ИСПРАВЛЕНО: Return type = PagesForVisualizationDto (синхронизировано с Handler)
using System;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Queries.Pages;

/// <summary>
/// Запрос страниц для визуализации
/// </summary>
public sealed record GetPagesForVisualizationQuery : IRequest<Result<PagesForVisualizationDto>>
{
    public Guid BookId { get; init; }
    public Guid? ChapterId { get; init; }
    public bool OnlyWithoutVisualization { get; init; }
    public bool OnlyVisualizationPoints { get; init; }
}