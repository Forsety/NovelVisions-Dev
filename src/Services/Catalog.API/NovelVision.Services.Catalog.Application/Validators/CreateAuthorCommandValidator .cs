using FluentValidation;
using NovelVision.Services.Catalog.Application.Commands.Authors;

namespace NovelVision.Services.Catalog.Application.Validators;

public class CreateAuthorCommandValidator : AbstractValidator<CreateAuthorCommand>
{
    public CreateAuthorCommandValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .MinimumLength(2).WithMessage("Display name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Display name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Biography)
            .MaximumLength(2000).WithMessage("Biography cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Biography));

        RuleFor(x => x.SocialLinks)
            .Must(sl => sl.Count <= 10)
            .WithMessage("Cannot have more than 10 social links");

        When(x => x.SocialLinks.Any(), () =>
        {
            RuleForEach(x => x.SocialLinks)
                .ChildRules(link =>
                {
                    link.RuleFor(l => l.Key)
                        .NotEmpty()
                        .MaximumLength(50);

                    link.RuleFor(l => l.Value)
                        .NotEmpty()
                        .Must(BeAValidUrl)
                        .WithMessage("Invalid URL format");
                });
        });
    }
    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

}