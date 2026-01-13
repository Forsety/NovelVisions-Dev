using FluentValidation;
using NovelVision.Services.Catalog.Application.Commands.Books;

namespace NovelVision.Services.Catalog.Application.Validators;

public class AddChapterCommandValidator : AbstractValidator<AddChapterCommand>
{
    public AddChapterCommandValidator()
    {
        RuleFor(x => x.BookId)
            .NotEmpty().WithMessage("Book ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Chapter title is required")
            .MinimumLength(1)
            .MaximumLength(200);

        RuleFor(x => x.Summary)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Summary));
    }
}
