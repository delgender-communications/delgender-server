using Core.DTOs.ResearchLeads;
using FluentValidation;

namespace Application.Validators
{
    public class DismissResearchLeadDtoValidator : AbstractValidator<DismissResearchLeadDto>
    {
        public DismissResearchLeadDtoValidator()
        {
            RuleFor(x => x.Reason).MaximumLength(500);
        }
    }
}
