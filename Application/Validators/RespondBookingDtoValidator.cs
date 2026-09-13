using Core.DTOs.Booking;
using Core.Enums;
using FluentValidation;

namespace Application.Validators
{
    public class RespondBookingDtoValidator : AbstractValidator<RespondBookingDto>
    {
        public RespondBookingDtoValidator()
        {
            RuleFor(x => x.Status)
                .Must(s => s == BookingStatus.Confirmed || s == BookingStatus.Declined)
                .WithMessage("A response must either confirm or decline the booking.");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("The email message to the client is required.")
                .MaximumLength(2000);

            RuleFor(x => x.DeclineReason)
                .NotEmpty().WithMessage("A reason for declining is required.")
                .When(x => x.Status == BookingStatus.Declined);
        }
    }
}
