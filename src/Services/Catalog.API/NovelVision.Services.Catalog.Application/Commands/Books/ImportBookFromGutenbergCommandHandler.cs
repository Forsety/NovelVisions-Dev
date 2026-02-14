// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Commands/Books/ImportBookFromGutenbergCommandHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs.Import;
using NovelVision.Services.Catalog.Application.Interfaces;

namespace NovelVision.Services.Catalog.Application.Commands.Books;

/// <summary>
/// Обработчик команды импорта книги из Gutenberg
/// </summary>
public class ImportBookFromGutenbergCommandHandler
    : IRequestHandler<ImportBookFromGutenbergCommand, Result<ImportBookResultDto>>
{
    private readonly IBookImportService _bookImportService;
    private readonly ILogger<ImportBookFromGutenbergCommandHandler> _logger;

    public ImportBookFromGutenbergCommandHandler(
        IBookImportService bookImportService,
        ILogger<ImportBookFromGutenbergCommandHandler> logger)
    {
        _bookImportService = bookImportService;
        _logger = logger;
    }

    public async Task<Result<ImportBookResultDto>> Handle(
        ImportBookFromGutenbergCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing import request for Gutenberg book {GutenbergId}",
            request.GutenbergId);

        var options = new ImportOptions
        {
            ImportFullText = request.ImportFullText,
            ParseChapters = request.ParseChapters,
            SkipExisting = request.SkipExisting
        };

        return await _bookImportService.ImportGutenbergBookAsync(
            request.GutenbergId,
            options,
            cancellationToken);
    }
}