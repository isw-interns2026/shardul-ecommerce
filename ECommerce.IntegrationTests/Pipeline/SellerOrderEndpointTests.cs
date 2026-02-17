using System.Net;
using System.Net.Http.Json;
using ECommerce.IntegrationTests.Fixtures;
using ECommerce.IntegrationTests.Helpers;
using ECommerce.Models.Domain.Entities;
using ECommerce.Models.DTO.Seller;
using FluentAssertions;

namespace ECommerce.IntegrationTests.Pipeline;

[Collection("Database")]
public class SellerOrderEndpointTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly PostgresFixture _fixture;

    public SellerOrderEndpointTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _factory = new CustomWebApplicationFactory(fixture.ConnectionString);
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetAllOrders_ReturnsOnlyThisSellersOrders()
    {
        await using var db = _fixture.CreateDbContext();
        var seller1 = TestDatabaseHelper.SeedSeller(db);
        var seller2 = TestDatabaseHelper.SeedSeller(db);
        var p1 = TestDatabaseHelper.SeedProduct(db, seller1.Id);
        var p2 = TestDatabaseHelper.SeedProduct(db, seller2.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        TestDatabaseHelper.SeedTransactionWithOrders(db, buyer, [(p1, 1)]);
        TestDatabaseHelper.SeedTransactionWithOrders(db, buyer, [(p2, 1)]);

        var client = _factory.CreateClient().Authenticate(seller1.Id, "Seller");
        var orders = await client.GetFromJsonAsync<List<SellerOrderResponseDto>>("/seller/orders");

        orders.Should().AllSatisfy(o => o.ProductId.Should().Be(p1.Id));
    }

    [Fact]
    public async Task GetOrderById_OwnOrder_Returns200()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        var (_, orders) = TestDatabaseHelper.SeedTransactionWithOrders(db, buyer, [(product, 1)]);

        var client = _factory.CreateClient().Authenticate(seller.Id, "Seller");
        var response = await client.GetAsync($"/seller/orders/{orders[0].Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetOrderById_OtherSellersOrder_Returns404()
    {
        await using var db = _fixture.CreateDbContext();
        var seller1 = TestDatabaseHelper.SeedSeller(db);
        var seller2 = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller2.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        var (_, orders) = TestDatabaseHelper.SeedTransactionWithOrders(db, buyer, [(product, 1)]);

        var client = _factory.CreateClient().Authenticate(seller1.Id, "Seller");
        var response = await client.GetAsync($"/seller/orders/{orders[0].Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateOrder_InTransitToDelivered_Returns200()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = TestDatabaseHelper.SeedFullCheckoutScenario(db);

        // Confirm reservation to move orders to InTransit
        var svc = new ECommerce.Services.Implementations.StockReservationService(db);
        await svc.ConfirmReservation(scenario.Transaction.Id);

        var order = scenario.Orders[0];

        var client = _factory.CreateClient().Authenticate(scenario.Seller.Id, "Seller");
        var response = await client.PatchAsJsonAsync($"/seller/orders/{order.Id}",
            new { Status = OrderStatus.Delivered });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateOrder_InvalidTransition_Returns422()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        var (_, orders) = TestDatabaseHelper.SeedTransactionWithOrders(db, buyer, [(product, 1)]);

        // Order is in AwaitingPayment — can't transition to Delivered
        var client = _factory.CreateClient().Authenticate(seller.Id, "Seller");
        var response = await client.PatchAsJsonAsync($"/seller/orders/{orders[0].Id}",
            new { Status = OrderStatus.Delivered });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateOrder_NonExistent_Returns404()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);

        var client = _factory.CreateClient().Authenticate(seller.Id, "Seller");
        var response = await client.PatchAsJsonAsync($"/seller/orders/{Guid.NewGuid()}",
            new { Status = OrderStatus.Delivered });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
