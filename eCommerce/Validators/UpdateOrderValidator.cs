using ECommerce.Models.DTO.Seller;
using FluentValidation;

namespace ECommerce.Validators
{
    public class UpdateOrderValidator : AbstractValidator<UpdateOrderDto>
    {
        public UpdateOrderValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum();
        }
    }
}
