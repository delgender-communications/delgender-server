using Core.DTOs.StaffRequests;
using Core.Enums;
using Core.Utilities;
using FluentValidation;

namespace Application.Validators
{
    public class CreateStaffChangeRequestDtoValidator : AbstractValidator<CreateStaffChangeRequestDto>
    {
        public CreateStaffChangeRequestDtoValidator()
        {
            RuleFor(x => x.Type).IsInEnum();

            RuleFor(x => x.NewValue)
                .NotEmpty().WithMessage("Enter the new value.")
                .MaximumLength(200);

            When(x => x.Type == ChangeRequestType.Name, () =>
            {
                RuleFor(x => x.NewValue).MaximumLength(100);

                RuleFor(x => x.NewSurname)
                    .NotEmpty().WithMessage("Enter your surname.")
                    .MaximumLength(100);
            });

            When(x => x.Type == ChangeRequestType.Email, () =>
            {
                RuleFor(x => x.NewValue)
                    .EmailAddress().WithMessage("That email doesn't look right.");
            });

            When(x => x.Type == ChangeRequestType.PhoneNumber, () =>
            {
                RuleFor(x => x.NewValue)
                    .Must(value => PhoneNumbers.TryNormalize(value, out _))
                    .WithMessage("Enter a 10-digit phone number, like 081 234 1234.");
            });
        }
    }
}
