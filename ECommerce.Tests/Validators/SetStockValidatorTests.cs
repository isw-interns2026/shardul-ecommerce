using ECommerce.Models.DTO.Seller;
using ECommerce.Validators;
using FluentAssertions;

namespace ECommerce.Tests.Validators;

public class SetStockValidatorTests
{
    private readonly SetStockValidator _validator = new();

    // #85
    [Fact]
    public void Validate_ZeroStock_Passes()
    {
        var dto = new SetStockDto { CountInStock = 0 };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    // #86
    [Fact]
    public void Validate_PositiveStock_Passes()
    {
        var dto = new SetStockDto { CountInStock = 50 };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    // #87
    [Fact]
    public void Validate_NegativeStock_Fails()
    {
        var dto = new SetStockDto { CountInStock = -1 };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CountInStock");
    }
}
