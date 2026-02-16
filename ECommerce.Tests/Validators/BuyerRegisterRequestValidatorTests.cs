using ECommerce.Models.DTO.Auth.Request;
using ECommerce.Tests.Helpers;
using ECommerce.Validators;
using FluentAssertions;

namespace ECommerce.Tests.Validators;

public class BuyerRegisterRequestValidatorTests
{
    private readonly BuyerRegisterRequestValidator _validator = new();

    // #25
    [Fact]
    public void Validate_ValidInput_Passes()
    {
        var dto = TestData.ValidBuyerRegisterDto();

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    // #26
    [Fact]
    public void Validate_EmptyName_Fails()
    {
        var dto = TestData.ValidBuyerRegisterDto();
        dto.Name = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    // #27
    [Fact]
    public void Validate_NameExceeds100Chars_Fails()
    {
        var dto = TestData.ValidBuyerRegisterDto();
        dto.Name = new string('a', 101);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    // #28
    [Fact]
    public void Validate_EmptyEmail_Fails()
    {
        var dto = TestData.ValidBuyerRegisterDto();
        dto.Email = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    // #29
    [Fact]
    public void Validate_InvalidEmailFormat_Fails()
    {
        var dto = TestData.ValidBuyerRegisterDto();
        dto.Email = "notanemail";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    // #30
    [Fact]
    public void Validate_EmailExceeds256Chars_Fails()
    {
        var dto = TestData.ValidBuyerRegisterDto();
        dto.Email = new string('a', 245) + "@example.com"; // 257 chars

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    // #31
    [Fact]
    public void Validate_EmptyPassword_Fails()
    {
        var dto = TestData.ValidBuyerRegisterDto();
        dto.Password = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    // #32
    [Fact]
    public void Validate_PasswordTooShort_Fails()
    {
        var dto = TestData.ValidBuyerRegisterDto();
        dto.Password = "1234567"; // 7 chars

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    // #33
    [Fact]
    public void Validate_PasswordExactly8Chars_Passes()
    {
        var dto = TestData.ValidBuyerRegisterDto();
        dto.Password = "12345678";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    // #34
    [Fact]
    public void Validate_PasswordExceeds128Chars_Fails()
    {
        var dto = TestData.ValidBuyerRegisterDto();
        dto.Password = new string('a', 129);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    // #35
    [Fact]
    public void Validate_EmptyAddress_Fails()
    {
        var dto = TestData.ValidBuyerRegisterDto();
        dto.Address = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Address");
    }

    // #36
    [Fact]
    public void Validate_AddressExceeds500Chars_Fails()
    {
        var dto = TestData.ValidBuyerRegisterDto();
        dto.Address = new string('a', 501);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Address");
    }
}
