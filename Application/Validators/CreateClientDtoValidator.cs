using Core.DTOs.Client;
using FluentValidation;

namespace Application.Validators
{
    public class CreateClientDtoValidator : AbstractValidator<CreateClientDto>
    {
        public CreateClientDtoValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().WithMessage("Contact name is required.").MaximumLength(100);
            RuleFor(x => x.CompanyName).NotEmpty().WithMessage("Company name is required.").MaximumLength(100);
            RuleFor(x => x.Industry).NotEmpty().WithMessage("Industry is required.").MaximumLength(100);
            RuleFor(x => x.JobTitle).MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.").EmailAddress();
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required.").Length(10);
        }
    }
}
