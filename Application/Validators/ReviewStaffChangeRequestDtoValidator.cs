using Core.DTOs.StaffRequests;
using FluentValidation;

namespace Application.Validators
{
    public class ReviewStaffChangeRequestDtoValidator : AbstractValidator<ReviewStaffChangeRequestDto>
    {
        public ReviewStaffChangeRequestDtoValidator()
        {
            RuleFor(x => x.Note).MaximumLength(500);
        }
    }
}
