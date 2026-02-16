using ECommerce.Tests.Helpers;
using ECommerce.Validators;
using FluentAssertions;

namespace ECommerce.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    // #48
    [Fact]
    public void Validate_ValidInput_Passes()
    {
        var dto = TestData.ValidLoginDto();

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    // #49
    [Fact]
    public void Validate_EmptyEmail_Fails()
    {
        var dto = TestData.ValidLoginDto();
        dto.Email = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    // #50
    [Fact]
    public void Validate_InvalidEmailFormat_Fails()
    {
        var dto = TestData.ValidLoginDto();
        dto.Email = "notanemail";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    // #51
    [Fact]
    public void Validate_EmailExceeds256Chars_Fails()
    {
        var dto = TestData.ValidLoginDto();
        dto.Email = new string('a', 245) + "@example.com";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    // #52
    [Fact]
    public void Validate_EmptyPassword_Fails()
    {
        var dto = TestData.ValidLoginDto();
        dto.Password = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    // #53
    [Fact]
    public void Validate_PasswordExceeds128Chars_Fails()
    {
        var dto = TestData.ValidLoginDto();
        dto.Password = new string('a', 129);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
