using ECommerce.Models.DTO.Seller;
using FluentValidation;

namespace ECommerce.Validators
{
    public class SetStockValidator : AbstractValidator<SetStockDto>
    {
        public SetStockValidator()
        {
            RuleFor(x => x.CountInStock)
                .GreaterThanOrEqualTo(0);
        }
    }
}
