using ECommerce.Models.DTO.Seller;
using FluentValidation;

namespace ECommerce.Validators
{
    public class UpdateProductValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductValidator()
        {
            RuleFor(x => x.Sku)
                .MaximumLength(50)
                .When(x => x.Sku is not null);

            RuleFor(x => x.Name)
                .MinimumLength(1)
                .MaximumLength(200)
                .When(x => x.Name is not null);

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .When(x => x.Price is not null);

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .When(x => x.Description is not null);

            RuleFor(x => x.ImageUrl)
                .MaximumLength(2048)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("ImageUrl must be a valid URL.")
                .When(x => x.ImageUrl is not null);
        }
    }
}
