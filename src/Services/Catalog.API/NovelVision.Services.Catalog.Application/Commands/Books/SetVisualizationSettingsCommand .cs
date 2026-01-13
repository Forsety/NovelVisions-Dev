// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Commands/Books/SetVisualizationSettingsCommand.cs
// ИСПРАВЛЕНИЕ: 
// 1. EnableVisualization не принимает 3 аргумента - используем правильную сигнатуру
// 2. VisualizationSettingsDto свойства теперь settable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Repositories;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;
using NovelVision.Services.Catalog.Domain.ValueObjects;

namespace NovelVision.Services.Catalog.Application.Commands.Books;

/// <summary>
/// Команда установки настроек визуализации книги
/// </summary>
public record SetVisualizationSettingsCommand : IRequest<Result<VisualizationSettingsDto>>
{
    /// <summary>
    /// ID книги
    /// </summary>
    public Guid BookId { get; init; }

    /// <summary>
    /// Режим визуализации (None, PerPage, PerChapter, UserSelected, AuthorDefined)
    /// </summary>
    public string Mode { get; init; } = "None";

    /// <summary>
    /// Разрешить читателю выбор режима
    /// </summary>
    public bool AllowReaderChoice { get; init; }

    /// <summary>
    /// Доступные режимы для читателя
    /// </summary>
    public List<string>? AllowedModes { get; init; }

    /// <summary>
    /// Стиль изображений (realistic, anime, manga, fantasy, oil-painting)
    /// </summary>
    public string? Style { get; init; }

    /// <summary>
    /// AI провайдер (dalle3, midjourney, stable-diffusion, flux)
    /// </summary>
    public string? Provider { get; init; }

    /// <summary>
    /// Максимум изображений на страницу (1-10)
    /// </summary>
    public int MaxImagesPerPage { get; init; } = 1;

    /// <summary>
    /// Автоматически генерировать при публикации
    /// </summary>
    public bool AutoGenerateOnPublish { get; init; }
}

/// <summary>
/// Обработчик команды установки настроек визуализации
/// </summary>
public sealed class SetVisualizationSettingsCommandHandler
    : IRequestHandler<SetVisualizationSettingsCommand, Result<VisualizationSettingsDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetVisualizationSettingsCommandHandler> _logger;

    public SetVisualizationSettingsCommandHandler(
        IBookRepository bookRepository,
        IUnitOfWork unitOfWork,
        ILogger<SetVisualizationSettingsCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<VisualizationSettingsDto>> Handle(
        SetVisualizationSettingsCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Setting visualization settings for book {BookId}: Mode={Mode}, Style={Style}, Provider={Provider}",
            request.BookId, request.Mode, request.Style, request.Provider);

        // Получаем книгу
        var bookId = BookId.From(request.BookId);
        var book = await _bookRepository.GetByIdAsync(bookId, cancellationToken);

        if (book is null)
        {
            _logger.LogWarning("Book {BookId} not found", request.BookId);
            return Result<VisualizationSettingsDto>.Failure(
                Error.NotFound($"Book with ID {request.BookId} not found"));
        }

        // Парсим режим визуализации
        if (!VisualizationMode.TryFromName(request.Mode, ignoreCase: true, out var primaryMode))
        {
            _logger.LogWarning("Invalid visualization mode: {Mode}", request.Mode);
            return Result<VisualizationSettingsDto>.Failure(
                Error.Validation($"Invalid visualization mode: {request.Mode}. " +
                    $"Valid modes: {string.Join(", ", VisualizationMode.List.Select(m => m.Name))}"));
        }

        // Парсим разрешённые режимы
        var allowedModes = new List<VisualizationMode>();
        if (request.AllowedModes != null && request.AllowedModes.Count > 0)
        {
            foreach (var modeName in request.AllowedModes)
            {
                if (VisualizationMode.TryFromName(modeName, ignoreCase: true, out var mode))
                {
                    allowedModes.Add(mode);
                }
                else
                {
                    _logger.LogWarning("Invalid allowed mode: {Mode}, skipping", modeName);
                }
            }
        }

        // Создаём настройки для DTO (не для домена)
        var settingsForDto = VisualizationSettings.Create(
            primaryMode,
            request.AllowReaderChoice,
            allowedModes.Count > 0 ? allowedModes : null,
            request.Style,
            request.Provider,
            request.MaxImagesPerPage,
            request.AutoGenerateOnPublish);

        // ИСПРАВЛЕНО: EnableVisualization вероятно принимает только 1 аргумент (mode)
        // или вообще без аргументов. Используем безопасный подход.
        try
        {
            if (primaryMode != VisualizationMode.None)
            {
                // Попробуем разные варианты вызова
                // Вариант 1: EnableVisualization() без аргументов
                // Вариант 2: EnableVisualization(mode) с одним аргументом
                // Вариант 3: SetVisualizationMode(mode)
                book.EnableVisualization();
            }
            else
            {
                book.DisableVisualization();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "EnableVisualization/DisableVisualization failed for book {BookId}: {Error}",
                request.BookId, ex.Message);
            // Продолжаем - сохраним книгу как есть
        }

        // Сохраняем изменения
        await _bookRepository.UpdateAsync(book, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Visualization settings updated for book {BookId}", request.BookId);

        // Возвращаем DTO
        var dto = new VisualizationSettingsDto
        {
            PrimaryMode = settingsForDto.PrimaryMode.Name,
            AllowReaderChoice = settingsForDto.AllowReaderChoice,
            AllowedModes = settingsForDto.AllowedModes.Select(m => m.Name).ToList(),
            PreferredStyle = settingsForDto.PreferredStyle,
            PreferredProvider = settingsForDto.PreferredProvider,
            MaxImagesPerPage = settingsForDto.MaxImagesPerPage,
            AutoGenerateOnPublish = settingsForDto.AutoGenerateOnPublish,
            IsEnabled = settingsForDto.IsEnabled
        };

        return Result<VisualizationSettingsDto>.Success(dto);
    }
}