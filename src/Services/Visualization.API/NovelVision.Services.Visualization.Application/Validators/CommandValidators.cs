using FluentValidation;
using NovelVision.Services.Visualization.Application.Commands.CancelVisualizationJob;
using NovelVision.Services.Visualization.Application.Commands.CreateVisualizationJob;
using NovelVision.Services.Visualization.Application.Commands.DeleteImage;
using NovelVision.Services.Visualization.Application.Commands.RetryVisualizationJob;
using NovelVision.Services.Visualization.Application.Commands.SelectImage;
using NovelVision.Services.Visualization.Domain.Enums;

namespace NovelVision.Services.Visualization.Application.Validators;

/// <summary>
/// Валидатор для CreatePageVisualizationCommand
/// </summary>
public sealed class CreatePageVisualizationCommandValidator 
    : AbstractValidator<CreatePageVisualizationCommand>
{
    public CreatePageVisualizationCommandValidator()
    {
        RuleFor(x => x.BookId)
            .NotEmpty()
            .WithMessage("BookId is required");

        RuleFor(x => x.PageId)
            .NotEmpty()
            .WithMessage("PageId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.PreferredProvider)
            .Must(BeValidProvider)
            .When(x => !string.IsNullOrEmpty(x.PreferredProvider))
            .WithMessage("Invalid AI provider");
    }

    private static bool BeValidProvider(string? provider)
    {
        if (string.IsNullOrEmpty(provider)) return true;
        return AIModelProvider.TryFromName(provider, true, out _) ||
               AIModelProvider.List.Any(p => p.ApiName.Equals(provider, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Валидатор для CreateTextSelectionVisualizationCommand
/// </summary>
public sealed class CreateTextSelectionVisualizationCommandValidator
    : AbstractValidator<CreateTextSelectionVisualizationCommand>
{
    public CreateTextSelectionVisualizationCommandValidator()
    {
        RuleFor(x => x.BookId)
            .NotEmpty()
            .WithMessage("BookId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.SelectedText)
            .NotEmpty()
            .WithMessage("Selected text is required")
            .MinimumLength(10)
            .WithMessage("Selected text must be at least 10 characters")
            .MaximumLength(5000)
            .WithMessage("Selected text must not exceed 5000 characters");

        RuleFor(x => x.StartPosition)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Start position must be non-negative");

        RuleFor(x => x.EndPosition)
            .GreaterThan(x => x.StartPosition)
            .WithMessage("End position must be greater than start position");

        RuleFor(x => x.PageId)
            .NotEmpty()
            .WithMessage("PageId is required");

        RuleFor(x => x.PreferredProvider)
            .Must(BeValidProvider)
            .When(x => !string.IsNullOrEmpty(x.PreferredProvider))
            .WithMessage("Invalid AI provider");
    }

    private static bool BeValidProvider(string? provider)
    {
        if (string.IsNullOrEmpty(provider)) return true;
        return AIModelProvider.TryFromName(provider, true, out _) ||
               AIModelProvider.List.Any(p => p.ApiName.Equals(provider, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Валидатор для StartAutoNovelGenerationCommand
/// </summary>
public sealed class StartAutoNovelGenerationCommandValidator
    : AbstractValidator<StartAutoNovelGenerationCommand>
{
    public StartAutoNovelGenerationCommandValidator()
    {
        RuleFor(x => x.BookId)
            .NotEmpty()
            .WithMessage("BookId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");
    }
}

/// <summary>
/// Валидатор для CancelVisualizationJobCommand
/// </summary>
public sealed class CancelVisualizationJobCommandValidator
    : AbstractValidator<CancelVisualizationJobCommand>
{
    public CancelVisualizationJobCommandValidator()
    {
        RuleFor(x => x.JobId)
            .NotEmpty()
            .WithMessage("JobId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("Reason must not exceed 500 characters");
    }
}

/// <summary>
/// Валидатор для RetryVisualizationJobCommand
/// </summary>
public sealed class RetryVisualizationJobCommandValidator
    : AbstractValidator<RetryVisualizationJobCommand>
{
    public RetryVisualizationJobCommandValidator()
    {
        RuleFor(x => x.JobId)
            .NotEmpty()
            .WithMessage("JobId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");
    }
}

/// <summary>
/// Валидатор для SelectImageCommand
/// </summary>
public sealed class SelectImageCommandValidator
    : AbstractValidator<SelectImageCommand>
{
    public SelectImageCommandValidator()
    {
        RuleFor(x => x.JobId)
            .NotEmpty()
            .WithMessage("JobId is required");

        RuleFor(x => x.ImageId)
            .NotEmpty()
            .WithMessage("ImageId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");
    }
}

/// <summary>
/// Валидатор для DeleteImageCommand
/// </summary>
public sealed class DeleteImageCommandValidator
    : AbstractValidator<DeleteImageCommand>
{
    public DeleteImageCommandValidator()
    {
        RuleFor(x => x.JobId)
            .NotEmpty()
            .WithMessage("JobId is required");

        RuleFor(x => x.ImageId)
            .NotEmpty()
            .WithMessage("ImageId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");
    }
}
