using FluentValidation;
using NovelVision.Services.Catalog.Application.Commands.Books;
using NovelVision.Services.Catalog.Domain.Rules;

namespace NovelVision.Services.Catalog.Application.Validators;

public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(BookBusinessRules.MinTitleLength)
            .MaximumLength(BookBusinessRules.MaxTitleLength);

        RuleFor(x => x.Subtitle)
            .MaximumLength(BookBusinessRules.MaxTitleLength)
            .When(x => !string.IsNullOrEmpty(x.Subtitle));

        RuleFor(x => x.Description)
            .MaximumLength(BookBusinessRules.MaxDescriptionLength)
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Language code is required")
            .Matches("^[a-z]{2}$").WithMessage("Language code must be 2 lowercase letters");

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("Author ID is required");

        RuleFor(x => x.ISBN)
            .Matches(@"^(?:ISBN(?:-1[03])?:?\s)?(?=[0-9X]{10}$|(?=(?:[0-9]+[-\s]){3})[-\s0-9X]{13}$|97[89][0-9]{10}$)")
            .WithMessage("Invalid ISBN format")
            .When(x => !string.IsNullOrEmpty(x.ISBN));

        RuleFor(x => x.Genres)
            .Must(g => g.Count <= BookBusinessRules.MaxGenresPerBook)
            .WithMessage($"Cannot have more than {BookBusinessRules.MaxGenresPerBook} genres");

        RuleFor(x => x.Tags)
            .Must(t => t.Count <= BookBusinessRules.MaxTagsPerBook)
            .WithMessage($"Cannot have more than {BookBusinessRules.MaxTagsPerBook} tags");

        RuleForEach(x => x.Tags)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.PublicationDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Publication date cannot be in the future")
            .When(x => x.PublicationDate.HasValue);
    }
}
