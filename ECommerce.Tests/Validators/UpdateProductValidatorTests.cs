using ECommerce.Models.DTO.Seller;
using ECommerce.Validators;
using FluentAssertions;

namespace ECommerce.Tests.Validators;

public class UpdateProductValidatorTests
{
    private readonly UpdateProductValidator _validator = new();

    // #70
    [Fact]
    public void Validate_AllNulls_Passes()
    {
        var dto = new UpdateProductDto();

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    // #71
    [Fact]
    public void Validate_EmptyStringSku_Fails()
    {
        var dto = new UpdateProductDto { Sku = "" };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Sku");
    }

    // #72
    [Fact]
    public void Validate_SkuExceeds50Chars_Fails()
    {
        var dto = new UpdateProductDto { Sku = new string('A', 51) };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Sku");
    }

    // #73
    [Fact]
    public void Validate_ValidSku_Passes()
    {
        var dto = new UpdateProductDto { Sku = "ABC-123" };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    // #74
    [Fact]
    public void Validate_EmptyStringName_Fails()
    {
        var dto = new UpdateProductDto { Name = "" };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    // #75
    [Fact]
    public void Validate_NameExceeds200Chars_Fails()
    {
        var dto = new UpdateProductDto { Name = new string('a', 201) };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    // #76
    [Fact]
    public void Validate_ValidName_Passes()
    {
        var dto = new UpdateProductDto { Name = "Updated Widget" };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    // #77
    [Fact]
    public void Validate_PriceZero_Fails()
    {
        var dto = new UpdateProductDto { Price = 0 };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    // #78
    [Fact]
    public void Validate_NegativePrice_Fails()
    {
        var dto = new UpdateProductDto { Price = -5m };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    // #79
    [Fact]
    public void Validate_ValidPrice_Passes()
    {
        var dto = new UpdateProductDto { Price = 19.99m };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    // #80
    [Fact]
    public void Validate_DescriptionExceeds2000Chars_Fails()
    {
        var dto = new UpdateProductDto { Description = new string('a', 2001) };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    // #81
    [Fact]
    public void Validate_ValidDescription_Passes()
    {
        var dto = new UpdateProductDto { Description = "A valid description" };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    // #82
    [Fact]
    public void Validate_InvalidImageUrl_Fails()
    {
        var dto = new UpdateProductDto { ImageUrl = "not-a-url" };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ImageUrl");
    }

    // #83
    [Fact]
    public void Validate_ImageUrlExceeds2048Chars_Fails()
    {
        var dto = new UpdateProductDto { ImageUrl = "https://example.com/" + new string('a', 2030) };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ImageUrl");
    }

    // #84
    [Fact]
    public void Validate_ValidImageUrl_Passes()
    {
        var dto = new UpdateProductDto { ImageUrl = "https://example.com/img.png" };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }
}
