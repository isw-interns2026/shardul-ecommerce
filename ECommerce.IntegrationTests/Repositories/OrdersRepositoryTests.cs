using ECommerce.IntegrationTests.Fixtures;
using ECommerce.Models.Domain.Entities;
using ECommerce.Repositories.Implementations;
using ECommerce.Repositories.Interfaces;
using FluentAssertions;

namespace ECommerce.IntegrationTests.Repositories;

[Collection("Database")]
public class OrdersRepositoryTests
{
    private readonly PostgresFixture _fixture;

    public OrdersRepositoryTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetOrders_BuyerScope_ReturnsOnlyBuyerOrders()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer1 = TestDatabaseHelper.SeedBuyer(db);
        var buyer2 = TestDatabaseHelper.SeedBuyer(db);

        TestDatabaseHelper.SeedTransactionWithOrders(db, buyer1, [(product, 1)]);
        TestDatabaseHelper.SeedTransactionWithOrders(db, buyer2, [(product, 1)]);

        var repo = new OrdersRepository(db);
        var result = await repo.GetOrdersAsync(new MandatoryUserIdArgument.Buyer([buyer1.Id]));

        result.Should().AllSatisfy(o => o.BuyerId.Should().Be(buyer1.Id));
    }

    [Fact]
    public async Task GetOrders_SellerScope_ReturnsOnlySellerOrders()
    {
        await using var db = _fixture.CreateDbContext();
        var seller1 = TestDatabaseHelper.SeedSeller(db);
        var seller2 = TestDatabaseHelper.SeedSeller(db);
        var p1 = TestDatabaseHelper.SeedProduct(db, seller1.Id);
        var p2 = TestDatabaseHelper.SeedProduct(db, seller2.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        TestDatabaseHelper.SeedTransactionWithOrders(db, buyer, [(p1, 1)]);
        TestDatabaseHelper.SeedTransactionWithOrders(db, buyer, [(p2, 1)]);

        var repo = new OrdersRepository(db);
        var result = await repo.GetOrdersAsync(new MandatoryUserIdArgument.Seller([seller1.Id]));

        result.Should().AllSatisfy(o => o.SellerId.Should().Be(seller1.Id));
    }

    [Fact]
    public async Task GetOrders_BuyerCantSeeOtherBuyerOrders()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer1 = TestDatabaseHelper.SeedBuyer(db);
        var buyer2 = TestDatabaseHelper.SeedBuyer(db);

        var (_, orders) = TestDatabaseHelper.SeedTransactionWithOrders(db, buyer2, [(product, 1)]);
        var orderId = orders[0].Id;

        var repo = new OrdersRepository(db);
        var result = await repo.GetOrdersAsync(
            new MandatoryUserIdArgument.Buyer([buyer1.Id]), orderIds: [orderId]);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOrders_SellerCantSeeOtherSellerOrders()
    {
        await using var db = _fixture.CreateDbContext();
        var seller1 = TestDatabaseHelper.SeedSeller(db);
        var seller2 = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller2.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        var (_, orders) = TestDatabaseHelper.SeedTransactionWithOrders(db, buyer, [(product, 1)]);

        var repo = new OrdersRepository(db);
        var result = await repo.GetOrdersAsync(
            new MandatoryUserIdArgument.Seller([seller1.Id]), orderIds: [orders[0].Id]);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOrders_FilterByOrderId_ReturnsSingleMatch()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        var (_, orders) = TestDatabaseHelper.SeedTransactionWithOrders(db, buyer, [(product, 1)]);

        var repo = new OrdersRepository(db);
        var result = await repo.GetOrdersAsync(
            new MandatoryUserIdArgument.Buyer([buyer.Id]), orderIds: [orders[0].Id]);

        result.Should().ContainSingle(o => o.Id == orders[0].Id);
    }

    [Fact]
    public async Task GetOrders_FilterByProductId_ReturnsMatchingOrders()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var p1 = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var p2 = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        TestDatabaseHelper.SeedTransactionWithOrders(db, buyer, [(p1, 1), (p2, 1)]);

        var repo = new OrdersRepository(db);
        var result = await repo.GetOrdersAsync(
            new MandatoryUserIdArgument.Buyer([buyer.Id]), productIds: [p1.Id]);

        result.Should().AllSatisfy(o => o.ProductId.Should().Be(p1.Id));
    }

    [Fact]
    public async Task GetOrders_IncludesProductNavigation()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        TestDatabaseHelper.SeedTransactionWithOrders(db, buyer, [(product, 1)]);

        var repo = new OrdersRepository(db);
        var result = await repo.GetOrdersAsync(new MandatoryUserIdArgument.Buyer([buyer.Id]));

        result.Should().AllSatisfy(o => o.Product.Should().NotBeNull());
    }
}
