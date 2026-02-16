using ECommerce.Tests.Helpers;
using ECommerce.Validators;
using FluentAssertions;

namespace ECommerce.Tests.Validators;

public class AddProductValidatorTests
{
    private readonly AddProductValidator _validator = new();

    // #54
    [Fact]
    public void Validate_ValidInputWithAllFields_Passes()
    {
        var dto = TestData.ValidAddProductDto();

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    // #55
    [Fact]
    public void Validate_EmptySku_Fails()
    {
        var dto = TestData.ValidAddProductDto();
        dto.Sku = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Sku");
    }

    // #56
    [Fact]
    public void Validate_SkuExceeds50Chars_Fails()
    {
        var dto = TestData.ValidAddProductDto();
        dto.Sku = new string('A', 51);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Sku");
    }

    // #57
    [Fact]
    public void Validate_EmptyName_Fails()
    {
        var dto = TestData.ValidAddProductDto();
        dto.Name = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    // #58
    [Fact]
    public void Validate_NameExceeds200Chars_Fails()
    {
        var dto = TestData.ValidAddProductDto();
        dto.Name = new string('a', 201);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    // #59
    [Fact]
    public void Validate_PriceZero_Fails()
    {
        var dto = TestData.ValidAddProductDto();
        dto.Price = 0;

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    // #60
    [Fact]
    public void Validate_NegativePrice_Fails()
    {
        var dto = TestData.ValidAddProductDto();
        dto.Price = -1;

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    // #61
    [Fact]
    public void Validate_CountInStockZero_Passes()
    {
        var dto = TestData.ValidAddProductDto();
        dto.CountInStock = 0;

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    // #62
    [Fact]
    public void Validate_NegativeCountInStock_Fails()
    {
        var dto = TestData.ValidAddProductDto();
        dto.CountInStock = -1;

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CountInStock");
    }

    // #63
    [Fact]
    public void Validate_NullDescription_Passes()
    {
        var dto = TestData.ValidAddProductDto();
        dto.Description = null;

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    // #64
    [Fact]
    public void Validate_DescriptionExceeds2000Chars_Fails()
    {
        var dto = TestData.ValidAddProductDto();
        dto.Description = new string('a', 2001);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    // #65
    [Fact]
    public void Validate_ValidDescription_Passes()
    {
        var dto = TestData.ValidAddProductDto();
        dto.Description = "A perfectly valid description.";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    // #66
    [Fact]
    public void Validate_NullImageUrl_Passes()
    {
        var dto = TestData.ValidAddProductDto();
        dto.ImageUrl = null;

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    // #67
    [Fact]
    public void Validate_InvalidImageUrl_Fails()
    {
        var dto = TestData.ValidAddProductDto();
        dto.ImageUrl = "not-a-url";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ImageUrl");
    }

    // #68
    [Fact]
    public void Validate_ImageUrlExceeds2048Chars_Fails()
    {
        var dto = TestData.ValidAddProductDto();
        dto.ImageUrl = "https://example.com/" + new string('a', 2030);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ImageUrl");
    }

    // #69
    [Fact]
    public void Validate_ValidImageUrl_Passes()
    {
        var dto = TestData.ValidAddProductDto();
        dto.ImageUrl = "https://example.com/image.png";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }
}
