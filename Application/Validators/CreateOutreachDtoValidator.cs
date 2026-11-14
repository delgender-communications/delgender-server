using Application.Services;
using Core.DTOs.Outreach;
using Core.Enums;
using FluentValidation;

namespace Application.Validators
{
    public class CreateOutreachDtoValidator : AbstractValidator<CreateOutreachDto>
    {
        public CreateOutreachDtoValidator()
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required.")
                .MaximumLength(200);

            RuleFor(x => x.ContactName).MaximumLength(120);
            RuleFor(x => x.PhoneNumber).MaximumLength(30);
            RuleFor(x => x.Notes).MaximumLength(4000);

            RuleFor(x => x.Email)
                .MaximumLength(200)
                .EmailAddress().WithMessage("That email doesn't look right.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Status).IsInEnum();

            RuleFor(x => x.Website)
                .MaximumLength(300)
                .Must(value => string.IsNullOrWhiteSpace(value)
                    || ResearchLeadNormalizer.TryNormalizeWebsite(value, out _, out _))
                .WithMessage("Enter a valid website, like example.com.");

            RuleFor(x => x.CancelReason).MaximumLength(200);
            RuleFor(x => x.CancelReason)
                .NotEmpty().WithMessage("Add a short reason for cancelling this company.")
                .When(x => x.Status == OutreachStatus.Cancelled);
        }
    }
}
