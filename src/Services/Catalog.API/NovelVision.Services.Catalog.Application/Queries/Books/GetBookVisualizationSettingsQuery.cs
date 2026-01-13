// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Queries/Books/GetBookVisualizationSettingsQuery.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Application.Queries.Books;

/// <summary>
/// Запрос настроек визуализации книги
/// </summary>
public record GetBookVisualizationSettingsQuery : IRequest<Result<VisualizationSettingsDto>>
{
    /// <summary>
    /// ID книги
    /// </summary>
    public Guid BookId { get; init; }
}

/// <summary>
/// Обработчик запроса настроек визуализации
/// </summary>
public sealed class GetBookVisualizationSettingsQueryHandler
    : IRequestHandler<GetBookVisualizationSettingsQuery, Result<VisualizationSettingsDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<GetBookVisualizationSettingsQueryHandler> _logger;

    public GetBookVisualizationSettingsQueryHandler(
        IBookRepository bookRepository,
        ILogger<GetBookVisualizationSettingsQueryHandler> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }

    public async Task<Result<VisualizationSettingsDto>> Handle(
        GetBookVisualizationSettingsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting visualization settings for book {BookId}", request.BookId);

        var bookId = BookId.From(request.BookId);
        var book = await _bookRepository.GetByIdAsync(bookId, cancellationToken);

        if (book is null)
        {
            _logger.LogWarning("Book {BookId} not found", request.BookId);
            return Result<VisualizationSettingsDto>.Failure(
                Error.NotFound($"Book with ID {request.BookId} not found"));
        }

        var settings = book.VisualizationSettings;
        var dto = new VisualizationSettingsDto
        {
            PrimaryMode = settings.PrimaryMode.Name,
            AllowReaderChoice = settings.AllowReaderChoice,
            AllowedModes = settings.AllowedModes.Select(m => m.Name).ToList(),
            PreferredStyle = settings.PreferredStyle,
            PreferredProvider = settings.PreferredProvider,
            MaxImagesPerPage = settings.MaxImagesPerPage,
            AutoGenerateOnPublish = settings.AutoGenerateOnPublish,
            IsEnabled = settings.IsEnabled
        };

        return Result<VisualizationSettingsDto>.Success(dto);
    }
}