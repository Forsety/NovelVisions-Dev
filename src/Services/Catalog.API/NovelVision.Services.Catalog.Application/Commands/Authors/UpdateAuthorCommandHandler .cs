// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Commands/Authors/UpdateAuthorCommandHandler.cs
// ИСПРАВЛЕНИЕ: AddSocialLink() и RemoveSocialLink() возвращают void
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
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Application.Commands.Authors;

public class UpdateAuthorCommandHandler : IRequestHandler<UpdateAuthorCommand, Result<AuthorDto>>
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateAuthorCommandHandler> _logger;

    public UpdateAuthorCommandHandler(
        IAuthorRepository authorRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateAuthorCommandHandler> logger)
    {
        _authorRepository = authorRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<AuthorDto>> Handle(UpdateAuthorCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating author: {AuthorId}", request.Id);

        var authorId = AuthorId.From(request.Id);
        var author = await _authorRepository.GetByIdAsync(authorId, cancellationToken);

        if (author is null)
        {
            return Result<AuthorDto>.Failure(Error.NotFound($"Author with ID {request.Id} not found"));
        }

        // Update profile - метод возвращает void
        author.UpdateProfile(request.DisplayName, request.Biography);

        // Update social links (remove all and re-add)
        // ИСПРАВЛЕНО: RemoveSocialLink возвращает void
        var platformsToRemove = author.SocialLinks.Keys.ToList();
        foreach (var platform in platformsToRemove)
        {
            try
            {
                author.RemoveSocialLink(platform);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove social link {Platform}", platform);
            }
        }

        // ИСПРАВЛЕНО: AddSocialLink возвращает void
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

        await _authorRepository.UpdateAsync(author, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Author updated successfully: {AuthorId}", request.Id);

        var authorDto = _mapper.Map<AuthorDto>(author);
        return Result<AuthorDto>.Success(authorDto);
    }
}