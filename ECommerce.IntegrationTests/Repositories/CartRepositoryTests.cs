using ECommerce.IntegrationTests.Fixtures;
using ECommerce.Repositories.Implementations;
using FluentAssertions;

namespace ECommerce.IntegrationTests.Repositories;

[Collection("Database")]
public class CartRepositoryTests
{
    private readonly PostgresFixture _fixture;

    public CartRepositoryTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task AddOrUpdateCart_NewItem_CreatesCartItem()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        var repo = new CartRepository(db);
        await repo.AddOrUpdateCartAsync(buyer.Id, product.Id, 3);
        await db.SaveChangesAsync();

        var items = await repo.GetBuyerCartItemsAsync(buyer.Id);
        items.Should().ContainSingle(ci => ci.ProductId == product.Id && ci.Count == 3);
    }

    [Fact]
    public async Task AddOrUpdateCart_ExistingItem_UpdatesCount()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, product.Id, count: 2);

        var repo = new CartRepository(db);
        await repo.AddOrUpdateCartAsync(buyer.Id, product.Id, 7);
        await db.SaveChangesAsync();

        var items = await repo.GetBuyerCartItemsAsync(buyer.Id);
        items.Should().ContainSingle(ci => ci.ProductId == product.Id && ci.Count == 7);
    }

    [Fact]
    public async Task GetBuyerCartItems_IncludesProduct()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, product.Id, count: 2);

        var repo = new CartRepository(db);
        var items = await repo.GetBuyerCartItemsAsync(buyer.Id);

        items.Should().ContainSingle();
        items[0].Product.Should().NotBeNull();
        items[0].Product.Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task DeleteProductFromCart_ExistingItem_Removes()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, product.Id, count: 2);

        var repo = new CartRepository(db);
        await repo.DeleteProductFromCartAsync(buyer.Id, product.Id);
        await db.SaveChangesAsync();

        var items = await repo.GetBuyerCartItemsAsync(buyer.Id);
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteProductFromCart_NonExistentItem_NoError()
    {
        await using var db = _fixture.CreateDbContext();
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        var repo = new CartRepository(db);
        await repo.DeleteProductFromCartAsync(buyer.Id, Guid.NewGuid());
        await db.SaveChangesAsync();
        // No exception
    }

    [Fact]
    public async Task ClearCart_RemovesAllItems()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var p1 = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var p2 = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, p1.Id, count: 1);
        TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, p2.Id, count: 2);

        var repo = new CartRepository(db);
        await repo.ClearCartAsync(buyer.Id);

        await using var verifyDb = _fixture.CreateDbContext();
        var items = await new CartRepository(verifyDb).GetBuyerCartItemsAsync(buyer.Id);
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBuyerCartItems_EmptyCart_ReturnsEmptyList()
    {
        await using var db = _fixture.CreateDbContext();
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        var repo = new CartRepository(db);
        var items = await repo.GetBuyerCartItemsAsync(buyer.Id);

        items.Should().BeEmpty();
    }
}
