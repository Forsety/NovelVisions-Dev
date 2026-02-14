// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Commands/Authors/VerifyAuthorCommandHandler.cs
// ИСПРАВЛЕНИЕ: Метод Verify() возвращает void, убрано присвоение переменной
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.BuildingBlocks.SharedKernel.Repositories;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Application.Commands.Authors;

public class VerifyAuthorCommandHandler : IRequestHandler<VerifyAuthorCommand, Result<bool>>
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyAuthorCommandHandler> _logger;

    public VerifyAuthorCommandHandler(
        IAuthorRepository authorRepository,
        IUnitOfWork unitOfWork,
        ILogger<VerifyAuthorCommandHandler> logger)
    {
        _authorRepository = authorRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(VerifyAuthorCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Verifying author: {AuthorId}", request.AuthorId);

        var authorId = AuthorId.From(request.AuthorId);
        var author = await _authorRepository.GetByIdAsync(authorId, cancellationToken);

        if (author is null)
        {
            return Result<bool>.Failure(Error.NotFound($"Author with ID {request.AuthorId} not found"));
        }

        // ИСПРАВЛЕНО: Verify() возвращает void, просто вызываем метод
        // Если нужна валидация, проверяем состояние автора после вызова
        try
        {
            author.Verify();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify author: {AuthorId}", request.AuthorId);
            return Result<bool>.Failure(Error.Failure($"Failed to verify author: {ex.Message}"));
        }

        await _authorRepository.UpdateAsync(author, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Author verified successfully: {AuthorId}", request.AuthorId);
        return Result<bool>.Success(true);
    }
}