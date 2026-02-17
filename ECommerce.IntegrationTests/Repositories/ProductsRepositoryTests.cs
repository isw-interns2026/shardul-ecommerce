using ECommerce.IntegrationTests.Fixtures;
using ECommerce.Repositories.Implementations;
using FluentAssertions;

namespace ECommerce.IntegrationTests.Repositories;

[Collection("Database")]
public class ProductsRepositoryTests
{
    private readonly PostgresFixture _fixture;

    public ProductsRepositoryTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetBySellerId_ReturnsOnlyThatSellersProducts()
    {
        await using var db = _fixture.CreateDbContext();
        var s1 = TestDatabaseHelper.SeedSeller(db);
        var s2 = TestDatabaseHelper.SeedSeller(db);
        var p1 = TestDatabaseHelper.SeedProduct(db, s1.Id);
        var p2 = TestDatabaseHelper.SeedProduct(db, s2.Id);

        var repo = new ProductsRepository(db);
        var result = await repo.GetProductsBySellerIdAsync([s1.Id]);

        result.Should().Contain(p => p.Id == p1.Id);
        result.Should().NotContain(p => p.Id == p2.Id);
    }

    [Fact]
    public async Task GetBySellerId_WithProductIdFilter_ReturnsSingleMatch()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var p1 = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var p2 = TestDatabaseHelper.SeedProduct(db, seller.Id);

        var repo = new ProductsRepository(db);
        var result = await repo.GetProductsBySellerIdAsync([seller.Id], productIds: [p1.Id]);

        result.Should().ContainSingle(p => p.Id == p1.Id);
    }

    [Fact]
    public async Task GetBySellerId_ProductBelongsToOtherSeller_Empty()
    {
        await using var db = _fixture.CreateDbContext();
        var s1 = TestDatabaseHelper.SeedSeller(db);
        var s2 = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, s2.Id);

        var repo = new ProductsRepository(db);
        var result = await repo.GetProductsBySellerIdAsync([s1.Id], productIds: [product.Id]);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllListed_ExcludesUnlistedProducts()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var listed = TestDatabaseHelper.SeedProduct(db, seller.Id, isListed: true);
        var unlisted = TestDatabaseHelper.SeedProduct(db, seller.Id, isListed: false);

        var repo = new ProductsRepository(db);
        var result = await repo.GetAllListedProductsAsync();

        result.Should().Contain(p => p.Id == listed.Id);
        result.Should().NotContain(p => p.Id == unlisted.Id);
    }

    [Fact]
    public async Task GetListedById_Listed_ReturnsProduct()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id, isListed: true);

        var repo = new ProductsRepository(db);
        var result = await repo.GetListedProductsByIdAsync(product.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task GetListedById_Unlisted_ReturnsNull()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id, isListed: false);

        var repo = new ProductsRepository(db);
        var result = await repo.GetListedProductsByIdAsync(product.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetListedById_NonExistent_ReturnsNull()
    {
        await using var db = _fixture.CreateDbContext();
        var repo = new ProductsRepository(db);
        var result = await repo.GetListedProductsByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }
}
