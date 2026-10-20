using Core.DTOs.Outreach;
using FluentValidation;

namespace Application.Validators
{
    public class UpdateOutreachNotesDtoValidator : AbstractValidator<UpdateOutreachNotesDto>
    {
        public UpdateOutreachNotesDtoValidator()
        {
            RuleFor(x => x.Notes).MaximumLength(4000);
        }
    }
}
