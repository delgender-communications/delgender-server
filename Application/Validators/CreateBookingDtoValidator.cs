using Core.DTOs;
using FluentValidation;

namespace Application.Validators
{
    public class CreateBookingDtoValidator : AbstractValidator<CreateBookingDto>
    {
        public CreateBookingDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Customer name is required.")
                .MaximumLength(100);

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required.")
                .MaximumLength(100);

            RuleFor(x => x.Industry)
                .NotEmpty().WithMessage("Industry is required.")
                .MaximumLength(100);

            RuleFor(x => x.JobTitle)
                .MaximumLength(100);

            RuleFor(x => x.HelpWith)
                .NotEmpty().WithMessage("What you need help with is required.")
                .MaximumLength(100);

            RuleFor(x => x.ProblemDescription)
                .NotEmpty().WithMessage("Problem description is required.")
                .MaximumLength(500);

            RuleFor(x => x.SessionGoal)
                .NotEmpty().WithMessage("Session outcome is required.")
                .MaximumLength(500);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Customer email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x)
                .Must(x =>
                {
                    var bookingDateTime = x.Date.ToDateTime(x.Time);
                    return bookingDateTime > DateTime.Now;
                })
                .WithMessage("Booking must be scheduled in the future.");


            RuleFor(x => x.Time)
                .Must(time => time >= new TimeOnly(9, 0) && time <= new TimeOnly(17, 0))
                .WithMessage("Booking only available during business hours (9:00 AM - 5:00 PM).");
        }
    }
}
