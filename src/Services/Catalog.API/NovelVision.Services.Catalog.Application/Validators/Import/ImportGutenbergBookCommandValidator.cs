// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Validators/Import/ImportGutenbergBookCommandValidator.cs
using FluentValidation;
using NovelVision.Services.Catalog.Application.Commands.Import;

namespace NovelVision.Services.Catalog.Application.Validators.Import;

public class ImportGutenbergBookCommandValidator : AbstractValidator<ImportGutenbergBookCommand>
{
    public ImportGutenbergBookCommandValidator()
    {
        RuleFor(x => x.GutenbergId)
            .GreaterThan(0)
            .WithMessage("Gutenberg ID must be a positive number");

        RuleFor(x => x.WordsPerPage)
            .InclusiveBetween(100, 1000)
            .WithMessage("Words per page must be between 100 and 1000");
    }
}

public class BulkImportGutenbergCommandValidator : AbstractValidator<BulkImportGutenbergCommand>
{
    public BulkImportGutenbergCommandValidator()
    {
        RuleFor(x => x.MaxBooks)
            .InclusiveBetween(1, 1000)
            .WithMessage("Max books must be between 1 and 1000");

        RuleFor(x => x.WordsPerPage)
            .InclusiveBetween(100, 1000)
            .WithMessage("Words per page must be between 100 and 1000");

        RuleFor(x => x.DelayBetweenRequests)
            .InclusiveBetween(0, 10000)
            .WithMessage("Delay must be between 0 and 10000 ms");

        RuleForEach(x => x.GutenbergIds)
            .GreaterThan(0)
            .WithMessage("All Gutenberg IDs must be positive");
    }
}