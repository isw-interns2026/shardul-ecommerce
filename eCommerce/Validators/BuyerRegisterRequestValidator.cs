using ECommerce.Models.DTO.Auth.Request;
using FluentValidation;

namespace ECommerce.Validators
{
    public class BuyerRegisterRequestValidator : AbstractValidator<BuyerRegisterRequestDto>
    {
        public BuyerRegisterRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(256);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(128);

            RuleFor(x => x.Address)
                .NotEmpty()
                .MaximumLength(500);
        }
    }
}
