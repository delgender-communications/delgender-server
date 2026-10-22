using Core.DTOs.ResearchLeads;
using FluentValidation;

namespace Application.Validators
{
    public class IngestResearchLeadsDtoValidator : AbstractValidator<IngestResearchLeadsDto>
    {
        public IngestResearchLeadsDtoValidator()
        {
            RuleFor(x => x.RunId).MaximumLength(100);

            RuleFor(x => x.Leads)
                .NotEmpty().WithMessage("Send at least one lead.")
                .Must(leads => leads.Count <= 100).WithMessage("Send at most 100 leads at a time.");

            RuleForEach(x => x.Leads).SetValidator(new ResearchLeadInputDtoValidator());
        }
    }
}
