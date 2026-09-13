using Core.DTOs.Invoice;
using FluentValidation;

namespace Application.Validators
{
    public class UpdateInvoiceDtoValidator : AbstractValidator<UpdateInvoiceDto>
    {
        public UpdateInvoiceDtoValidator()
        {
            RuleFor(x => x.Items).NotEmpty().WithMessage("At least one line item is required.");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.Description).NotEmpty().MaximumLength(300);
                item.RuleFor(i => i.Quantity).GreaterThan(0);
                item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
                item.RuleFor(i => i.TaxRate).InclusiveBetween(0, 100);
                item.RuleFor(i => i.DiscountAmount).GreaterThanOrEqualTo(0);
            });
        }
    }
}
