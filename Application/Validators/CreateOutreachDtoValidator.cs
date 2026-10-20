using Core.DTOs.Outreach;
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
        }
    }
}
