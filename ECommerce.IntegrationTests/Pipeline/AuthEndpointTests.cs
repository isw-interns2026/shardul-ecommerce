using System.Net;
using System.Net.Http.Json;
using ECommerce.IntegrationTests.Fixtures;
using FluentAssertions;

namespace ECommerce.IntegrationTests.Pipeline;

[Collection("Database")]
public class AuthEndpointTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly PostgresFixture _fixture;

    public AuthEndpointTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _factory = new CustomWebApplicationFactory(fixture.ConnectionString);
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task RegisterBuyer_ValidData_Returns200()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/auth/buyer/register", new
        {
            Name = "Test Buyer",
            Email = $"{Guid.NewGuid():N}@test.com",
            Password = "Test1234!",
            Address = "123 Test St"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RegisterBuyer_DuplicateEmail_Returns409()
    {
        var email = $"{Guid.NewGuid():N}@test.com";
        var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/auth/buyer/register", new
        {
            Name = "First", Email = email, Password = "Test1234!", Address = "123 St"
        });

        var response = await client.PostAsJsonAsync("/auth/buyer/register", new
        {
            Name = "Second", Email = email, Password = "Test1234!", Address = "456 St"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task RegisterBuyer_InvalidBody_Returns400WithValidationErrors()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/auth/buyer/register", new
        {
            Name = "",       // fails NotEmpty
            Email = "bad",   // fails EmailAddress
            Password = "123",// fails MinLength(8)
            Address = ""     // fails NotEmpty
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        body.Should().ContainKey("errors");
    }

    [Fact]
    public async Task RegisterSeller_ValidData_Returns200()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/auth/seller/register", new
        {
            Name = "Test Seller",
            Email = $"{Guid.NewGuid():N}@test.com",
            Password = "Test1234!",
            BankAccountNumber = "GB82WEST12345698765432"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RegisterSeller_DuplicateEmail_Returns409()
    {
        var email = $"{Guid.NewGuid():N}@test.com";
        var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/auth/seller/register", new
        {
            Name = "First", Email = email, Password = "Test1234!",
            BankAccountNumber = "GB82WEST12345698765432"
        });

        var response = await client.PostAsJsonAsync("/auth/seller/register", new
        {
            Name = "Second", Email = email, Password = "Test1234!",
            BankAccountNumber = "GB82WEST12345698765432"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task LoginBuyer_ValidCredentials_ReturnsAccessToken()
    {
        var email = $"{Guid.NewGuid():N}@test.com";
        var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/auth/buyer/register", new
        {
            Name = "Buyer", Email = email, Password = "Test1234!", Address = "123 St"
        });

        var response = await client.PostAsJsonAsync("/auth/buyer/login", new
        {
            Email = email, Password = "Test1234!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        body.Should().ContainKey("accessToken");
        body!["accessToken"].Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task LoginBuyer_WrongPassword_Returns401()
    {
        var email = $"{Guid.NewGuid():N}@test.com";
        var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/auth/buyer/register", new
        {
            Name = "Buyer", Email = email, Password = "Test1234!", Address = "123 St"
        });

        var response = await client.PostAsJsonAsync("/auth/buyer/login", new
        {
            Email = email, Password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginBuyer_NonExistentEmail_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/auth/buyer/login", new
        {
            Email = "nobody@test.com", Password = "Test1234!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginSeller_ValidCredentials_ReturnsAccessToken()
    {
        var email = $"{Guid.NewGuid():N}@test.com";
        var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/auth/seller/register", new
        {
            Name = "Seller", Email = email, Password = "Test1234!",
            BankAccountNumber = "GB82WEST12345698765432"
        });

        var response = await client.PostAsJsonAsync("/auth/seller/login", new
        {
            Email = email, Password = "Test1234!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        body.Should().ContainKey("accessToken");
    }
}
