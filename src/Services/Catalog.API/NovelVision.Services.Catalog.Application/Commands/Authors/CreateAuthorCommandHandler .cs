// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Commands/Authors/CreateAuthorCommandHandler.cs
// ИСПРАВЛЕНО: Author.Create() возвращает Result<Author>, нужно разворачивать
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.BuildingBlocks.SharedKernel.Repositories;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Aggregates.AuthorAggregate;
using NovelVision.Services.Catalog.Domain.Repositories;

namespace NovelVision.Services.Catalog.Application.Commands.Authors;

public class CreateAuthorCommandHandler : IRequestHandler<CreateAuthorCommand, Result<AuthorDto>>
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateAuthorCommandHandler> _logger;

    public CreateAuthorCommandHandler(
        IAuthorRepository authorRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateAuthorCommandHandler> logger)
    {
        _authorRepository = authorRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<AuthorDto>> Handle(CreateAuthorCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new author: {DisplayName}", request.DisplayName);

        // Check email uniqueness
        var emailUnique = await _authorRepository.IsEmailUniqueAsync(request.Email, null, cancellationToken);
        if (!emailUnique)
        {
            return Result<AuthorDto>.Failure(Error.Conflict($"Author with email {request.Email} already exists"));
        }

        // Author.Create возвращает Result<Author>
        var authorResult = Author.Create(
            request.DisplayName,
            request.Email,
            request.Biography);

        // Проверяем результат создания
        if (authorResult.IsFailed)
        {
            _logger.LogError("Failed to create author: {DisplayName}", request.DisplayName);
            return Result<AuthorDto>.Failure(authorResult.Errors.FirstOrDefault()
                ?? Error.Validation("Failed to create author"));
        }

        var author = authorResult.Value;

        // AddSocialLink возвращает void, но может бросить exception
        foreach (var (platform, url) in request.SocialLinks)
        {
            try
            {
                author.AddSocialLink(platform, url);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add social link {Platform}: {Error}",
                    platform, ex.Message);
            }
        }

        // Save to repository
        await _authorRepository.AddAsync(author, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Author created successfully with ID: {AuthorId}", author.Id.Value);

        // Map to DTO
        var authorDto = _mapper.Map<AuthorDto>(author);
        return Result<AuthorDto>.Success(authorDto);
    }
}