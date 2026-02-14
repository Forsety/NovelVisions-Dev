// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Commands/Import/ImportGutenbergBookCommand.cs
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs.Import;

namespace NovelVision.Services.Catalog.Application.Commands.Import;

/// <summary>
/// Команда импорта одной книги из Project Gutenberg
/// </summary>
public record ImportGutenbergBookCommand : IRequest<Result<ImportBookResultDto>>
{
    /// <summary>
    /// ID книги в Project Gutenberg
    /// </summary>
    public int GutenbergId { get; init; }

    /// <summary>
    /// Импортировать полный текст книги
    /// </summary>
    public bool ImportFullText { get; init; } = true;

    /// <summary>
    /// Количество слов на страницу при разбивке
    /// </summary>
    public int WordsPerPage { get; init; } = 300;

    /// <summary>
    /// Автоматически создавать автора если не существует
    /// </summary>
    public bool CreateAuthorIfNotExists { get; init; } = true;

    /// <summary>
    /// Автоматически создавать категории если не существуют
    /// </summary>
    public bool CreateSubjectsIfNotExist { get; init; } = true;

    /// <summary>
    /// Пропустить если книга уже существует
    /// </summary>
    public bool SkipIfExists { get; init; } = true;

    /// <summary>
    /// ID пользователя, выполняющего импорт
    /// </summary>
    public Guid? UserId { get; init; }
}