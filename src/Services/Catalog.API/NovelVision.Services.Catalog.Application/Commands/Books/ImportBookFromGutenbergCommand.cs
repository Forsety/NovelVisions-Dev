// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Commands/Books/ImportBookFromGutenbergCommand.cs
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs.Import;

namespace NovelVision.Services.Catalog.Application.Commands.Books;

/// <summary>
/// Команда импорта книги из Gutenberg
/// </summary>
public record ImportBookFromGutenbergCommand : IRequest<Result<ImportBookResultDto>>
{
    /// <summary>
    /// ID книги в Project Gutenberg
    /// </summary>
    public int GutenbergId { get; init; }

    /// <summary>
    /// Импортировать полный текст книги
    /// </summary>
    public bool ImportFullText { get; init; } = false;

    /// <summary>
    /// Разбивать текст на главы
    /// </summary>
    public bool ParseChapters { get; init; } = true;

    /// <summary>
    /// Пропустить если уже существует
    /// </summary>
    public bool SkipExisting { get; init; } = true;
}