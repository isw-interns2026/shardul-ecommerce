using ECommerce.Services.Implementations;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ECommerce.Tests.Services;

public class TokenServiceTests
{
    private const string SecretKey = "ThisIsASecretKeyForTestingPurposes1234567890!";
    private const string Issuer = "TestIssuer";
    private const string Audience = "TestAudience";

    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JWT:SecretKey"] = SecretKey,
                ["JWT:Issuer"] = Issuer,
                ["JWT:Audience"] = Audience
            })
            .Build();

        _tokenService = new TokenService(config);
    }

    private static JwtSecurityToken ParseToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        return handler.ReadJwtToken(token);
    }

    // #143
    [Fact]
    public void GenerateJWT_ContainsNameIdentifierClaim()
    {
        var token = _tokenService.GenerateJWT("user-123", "u@test.com", "User", "Buyer");

        var jwt = ParseToken(token);
        // JwtSecurityTokenHandler uses short claim names (nameid, not the full URI)
        jwt.Claims.Should().Contain(c =>
            c.Type == "nameid" && c.Value == "user-123");
    }

    // #144
    [Fact]
    public void GenerateJWT_ContainsRoleClaim()
    {
        var token = _tokenService.GenerateJWT("user-123", "u@test.com", "User", "Seller");

        var jwt = ParseToken(token);
        jwt.Claims.Should().Contain(c =>
            c.Type == "role" && c.Value == "Seller");
    }

    // #145
    [Fact]
    public void GenerateJWT_ContainsEmailClaim()
    {
        var token = _tokenService.GenerateJWT("user-123", "alice@test.com", "Alice", "Buyer");

        var jwt = ParseToken(token);
        jwt.Claims.Should().Contain(c =>
            c.Type == "email" && c.Value == "alice@test.com");
    }

    // #146
    [Fact]
    public void GenerateJWT_ContainsGivenNameClaim()
    {
        var token = _tokenService.GenerateJWT("user-123", "u@test.com", "Bob", "Buyer");

        var jwt = ParseToken(token);
        jwt.Claims.Should().Contain(c =>
            c.Type == "given_name" && c.Value == "Bob");
    }

    // #147
    [Fact]
    public void GenerateJWT_IssuerMatchesConfiguration()
    {
        var token = _tokenService.GenerateJWT("user-123", "u@test.com", "User", "Buyer");

        var jwt = ParseToken(token);
        jwt.Issuer.Should().Be(Issuer);
    }

    // #148
    [Fact]
    public void GenerateJWT_AudienceMatchesConfiguration()
    {
        var token = _tokenService.GenerateJWT("user-123", "u@test.com", "User", "Buyer");

        var jwt = ParseToken(token);
        jwt.Audiences.Should().Contain(Audience);
    }

    // #149
    [Fact]
    public void GenerateJWT_ExpiryIsApproximatelyOneDayFromNow()
    {
        var before = DateTime.UtcNow.AddDays(1).AddMinutes(-5);
        var token = _tokenService.GenerateJWT("user-123", "u@test.com", "User", "Buyer");
        var after = DateTime.UtcNow.AddDays(1).AddMinutes(5);

        var jwt = ParseToken(token);
        jwt.ValidTo.Should().BeOnOrAfter(before);
        jwt.ValidTo.Should().BeOnOrBefore(after);
    }

    // #150
    [Fact]
    public void GenerateJWT_TwoTokensHaveDifferentJti()
    {
        var token1 = _tokenService.GenerateJWT("user-1", "a@test.com", "A", "Buyer");
        var token2 = _tokenService.GenerateJWT("user-2", "b@test.com", "B", "Buyer");

        var jti1 = ParseToken(token1).Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = ParseToken(token2).Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        jti1.Should().NotBe(jti2);
    }
}
