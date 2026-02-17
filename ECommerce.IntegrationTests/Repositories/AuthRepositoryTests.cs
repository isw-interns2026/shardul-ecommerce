using ECommerce.IntegrationTests.Fixtures;
using ECommerce.Repositories.Implementations;
using FluentAssertions;

namespace ECommerce.IntegrationTests.Repositories;

[Collection("Database")]
public class AuthRepositoryTests
{
    private readonly PostgresFixture _fixture;

    public AuthRepositoryTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetBuyerIfValidCredentials_ValidCredentials_ReturnsBuyer()
    {
        await using var db = _fixture.CreateDbContext();
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        var email = buyer.Email;

        var repo = new AuthRepository(db);
        var result = await repo.GetBuyerIfValidCredentialsAsync(email, "Test1234!");

        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetBuyerIfValidCredentials_WrongPassword_ReturnsNull()
    {
        await using var db = _fixture.CreateDbContext();
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        var repo = new AuthRepository(db);
        var result = await repo.GetBuyerIfValidCredentialsAsync(buyer.Email, "WrongPassword!");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBuyerIfValidCredentials_NonExistentEmail_ReturnsNull()
    {
        await using var db = _fixture.CreateDbContext();
        var repo = new AuthRepository(db);
        var result = await repo.GetBuyerIfValidCredentialsAsync("nobody@test.com", "Test1234!");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSellerIfValidCredentials_ValidCredentials_ReturnsSeller()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);

        var repo = new AuthRepository(db);
        var result = await repo.GetSellerIfValidCredentialsAsync(seller.Email, "Test1234!");

        result.Should().NotBeNull();
        result!.Email.Should().Be(seller.Email);
    }

    [Fact]
    public async Task GetSellerIfValidCredentials_WrongPassword_ReturnsNull()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);

        var repo = new AuthRepository(db);
        var result = await repo.GetSellerIfValidCredentialsAsync(seller.Email, "WrongPassword!");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSellerIfValidCredentials_NonExistentEmail_ReturnsNull()
    {
        await using var db = _fixture.CreateDbContext();
        var repo = new AuthRepository(db);
        var result = await repo.GetSellerIfValidCredentialsAsync("nobody@test.com", "Test1234!");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBuyerByIdAsync_ReturnsCorrectBuyer()
    {
        await using var db = _fixture.CreateDbContext();
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        var repo = new AuthRepository(db);
        var result = await repo.GetBuyerByIdAsync(buyer.Id);

        result.Id.Should().Be(buyer.Id);
        result.Name.Should().Be(buyer.Name);
    }
}
