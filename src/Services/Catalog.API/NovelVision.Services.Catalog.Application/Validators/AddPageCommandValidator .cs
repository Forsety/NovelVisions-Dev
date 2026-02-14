using FluentValidation;
using NovelVision.Services.Catalog.Application.Commands.Books;

namespace NovelVision.Services.Catalog.Application.Validators;

public class AddPageCommandValidator : AbstractValidator<AddPageCommand>
{
    public AddPageCommandValidator()
    {
        RuleFor(x => x.BookId)
            .NotEmpty().WithMessage("Book ID is required");

        RuleFor(x => x.ChapterId)
            .NotEmpty().WithMessage("Chapter ID is required");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Page content is required")
            .MinimumLength(10).WithMessage("Page content must be at least 10 characters")
            .MaximumLength(50000).WithMessage("Page content cannot exceed 50,000 characters");
    }
}
