// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Commands/Import/BulkImportGutenbergCommand.cs
using System;
using System.Collections.Generic;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs.Import;

namespace NovelVision.Services.Catalog.Application.Commands.Import;

/// <summary>
/// Команда массового импорта книг из Project Gutenberg
/// </summary>
public record BulkImportGutenbergCommand : IRequest<Result<BulkImportResultDto>>
{
    /// <summary>
    /// Конкретные ID книг для импорта
    /// </summary>
    public List<int> GutenbergIds { get; init; } = new();

    /// <summary>
    /// Критерии поиска (если ID не указаны)
    /// </summary>
    public GutenbergSearchCriteriaDto? SearchCriteria { get; init; }

    /// <summary>
    /// Максимальное количество книг для импорта
    /// </summary>
    public int MaxBooks { get; init; } = 100;

    /// <summary>
    /// Импортировать полный текст книг
    /// </summary>
    public bool ImportFullText { get; init; } = true;

    /// <summary>
    /// Количество слов на страницу
    /// </summary>
    public int WordsPerPage { get; init; } = 300;

    /// <summary>
    /// Пропускать уже существующие книги
    /// </summary>
    public bool SkipExisting { get; init; } = true;

    /// <summary>
    /// Продолжать при ошибках
    /// </summary>
    public bool ContinueOnError { get; init; } = true;

    /// <summary>
    /// Задержка между запросами (мс)
    /// </summary>
    public int DelayBetweenRequests { get; init; } = 500;

    /// <summary>
    /// ID пользователя
    /// </summary>
    public Guid? UserId { get; init; }
}