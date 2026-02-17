using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace ECommerce.IntegrationTests.Helpers;

public static class HttpClientExtensions
{
    private const string TestSecretKey = "ThisIsATestSecretKeyThatIsLongEnoughForHmacSha256!!";
    private const string TestIssuer = "test-issuer";
    private const string TestAudience = "test-audience";

    public static HttpClient Authenticate(this HttpClient client, Guid userId, string role)
    {
        var token = GenerateJwt(userId, role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static string GenerateJwt(Guid userId, string role)
    {
        var key = Encoding.UTF8.GetBytes(TestSecretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, "test@test.com"),
                new Claim(ClaimTypes.GivenName, "Test"),
                new Claim(ClaimTypes.Role, role)
            ]),
            IssuedAt = DateTime.UtcNow,
            Issuer = TestIssuer,
            Audience = TestAudience,
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }
}
