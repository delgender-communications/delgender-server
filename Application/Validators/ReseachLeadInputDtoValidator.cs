using Application.Services;
using Core.DTOs.ResearchLeads;
using FluentValidation;

namespace Application.Validators
{
    public class ResearchLeadInputDtoValidator : AbstractValidator<ResearchLeadInputDto>
    {
        public ResearchLeadInputDtoValidator()
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required.")
                .MaximumLength(200)
                .Must(name => ResearchLeadNormalizer.NameKey(name).Length > 0)
                .WithMessage("That company name doesn't look right.");

            RuleFor(x => x.Website)
                .MaximumLength(300)
                .Must(site => ResearchLeadNormalizer.TryNormalizeWebsite(site, out _, out _))
                .WithMessage("That website doesn't look right.")
                .When(x => !string.IsNullOrWhiteSpace(x.Website));

            RuleFor(x => x.Industry).MaximumLength(120);
            RuleFor(x => x.Location).MaximumLength(120);
            RuleFor(x => x.PhoneNumber).MaximumLength(30);
            RuleFor(x => x.Summary).MaximumLength(1000);

            RuleFor(x => x.Email)
                .MaximumLength(200)
                .EmailAddress().WithMessage("That email doesn't look right.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Case).IsInEnum();

            RuleFor(x => x.Score)
                .InclusiveBetween(0, 100).WithMessage("Score must be between 0 and 100.")
                .When(x => x.Score.HasValue);

            RuleFor(x => x.Evidence)
                .Must(items => items.Count <= 10).WithMessage("Up to 10 pieces of evidence per lead.");

            RuleForEach(x => x.Evidence).ChildRules(evidence =>
            {
                evidence.RuleFor(e => e.Finding)
                    .NotEmpty().WithMessage("Each piece of evidence needs a finding.")
                    .MaximumLength(500);

                // Only real web links, so nothing like javascript: can end up behind a link in the portal.
                evidence.RuleFor(e => e.SourceUrl)
                    .MaximumLength(1000)
                    .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                                 (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                    .WithMessage("Evidence links must be web addresses starting with http or https.")
                    .When(e => !string.IsNullOrWhiteSpace(e.SourceUrl));
            });
        }
    }
}
