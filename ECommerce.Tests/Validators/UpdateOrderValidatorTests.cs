using ECommerce.Models.Domain.Entities;
using ECommerce.Models.DTO.Seller;
using ECommerce.Validators;
using FluentAssertions;

namespace ECommerce.Tests.Validators;

public class UpdateOrderValidatorTests
{
    private readonly UpdateOrderValidator _validator = new();

    // #88
    [Fact]
    public void Validate_ValidEnumValue_Passes()
    {
        var dto = new UpdateOrderDto { Status = OrderStatus.Delivered };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    // #89
    [Fact]
    public void Validate_InvalidEnumValue_Fails()
    {
        var dto = new UpdateOrderDto { Status = (OrderStatus)999 };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Status");
    }
}
