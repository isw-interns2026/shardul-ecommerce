using ECommerce.Models.DTO.Auth.Request;
using ECommerce.Tests.Helpers;
using ECommerce.Validators;
using FluentAssertions;

namespace ECommerce.Tests.Validators;

public class SellerRegisterRequestValidatorTests
{
    private readonly SellerRegisterRequestValidator _validator = new();

    // #37
    [Fact]
    public void Validate_ValidInput_Passes()
    {
        var dto = TestData.ValidSellerRegisterDto();

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    // #38
    [Fact]
    public void Validate_EmptyName_Fails()
    {
        var dto = TestData.ValidSellerRegisterDto();
        dto.Name = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    // #39
    [Fact]
    public void Validate_NameExceeds100Chars_Fails()
    {
        var dto = TestData.ValidSellerRegisterDto();
        dto.Name = new string('a', 101);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    // #40
    [Fact]
    public void Validate_EmptyEmail_Fails()
    {
        var dto = TestData.ValidSellerRegisterDto();
        dto.Email = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    // #41
    [Fact]
    public void Validate_InvalidEmailFormat_Fails()
    {
        var dto = TestData.ValidSellerRegisterDto();
        dto.Email = "notanemail";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    // #42
    [Fact]
    public void Validate_EmailExceeds256Chars_Fails()
    {
        var dto = TestData.ValidSellerRegisterDto();
        dto.Email = new string('a', 245) + "@example.com";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    // #43
    [Fact]
    public void Validate_EmptyPassword_Fails()
    {
        var dto = TestData.ValidSellerRegisterDto();
        dto.Password = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    // #44
    [Fact]
    public void Validate_PasswordTooShort_Fails()
    {
        var dto = TestData.ValidSellerRegisterDto();
        dto.Password = "1234567";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    // #45
    [Fact]
    public void Validate_PasswordExceeds128Chars_Fails()
    {
        var dto = TestData.ValidSellerRegisterDto();
        dto.Password = new string('a', 129);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    // #46
    [Fact]
    public void Validate_EmptyBankAccount_Fails()
    {
        var dto = TestData.ValidSellerRegisterDto();
        dto.BankAccountNumber = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BankAccountNumber");
    }

    // #47
    [Fact]
    public void Validate_BankAccountExceeds34Chars_Fails()
    {
        var dto = TestData.ValidSellerRegisterDto();
        dto.BankAccountNumber = new string('1', 35);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BankAccountNumber");
    }
}
