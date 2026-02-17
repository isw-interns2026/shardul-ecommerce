using System.Net;
using System.Net.Http.Json;
using ECommerce.Data;
using ECommerce.IntegrationTests.Fixtures;
using ECommerce.IntegrationTests.Helpers;
using ECommerce.Models.Domain.Entities;
using ECommerce.Services.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.IntegrationTests.Flows;

[Collection("Database")]
public class CheckoutFlowTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly PostgresFixture _fixture;

    public CheckoutFlowTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _factory = new CustomWebApplicationFactory(fixture.ConnectionString);
    }

    public void Dispose() => _factory.Dispose();

    /// <summary>
    /// Places an order via the HTTP pipeline and returns the transaction ID
    /// created in the database. Uses the Stripe mock which sets StripeSessionId.
    /// </summary>
    private async Task<Guid> PlaceOrderViaHttp(Guid buyerId, HttpClient client)
    {
        var response = await client.PostAsync("/buyer/cart", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Find the transaction created for this buyer's orders
        await using var db = _fixture.CreateDbContext();
        var tx = await db.Transactions
            .Where(t => t.Status == TransactionStatus.Processing)
            .OrderByDescending(t => t.Id) // UUIDv7 = chronological
            .FirstAsync();

        return tx.Id;
    }

    [Fact]
    public async Task FullHappyPath_Register_Login_AddToCart_PlaceOrder_Confirm()
    {
        // Register buyer
        var email = $"{Guid.NewGuid():N}@test.com";
        var httpClient = _factory.CreateClient();

        await httpClient.PostAsJsonAsync("/auth/buyer/register", new
        {
            Name = "Flow Buyer", Email = email, Password = "Test1234!", Address = "123 Flow St"
        });

        // Seed seller + product via DB (seller registration not buyer-accessible)
        await using var seedDb = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(seedDb);
        var product = TestDatabaseHelper.SeedProduct(seedDb, seller.Id, stock: 50);
        var buyer = await seedDb.Buyers.FirstAsync(b => b.Email == email);

        // Authenticate and add to cart
        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        await client.PostAsync($"/buyer/cart/{product.Id}?count=3", null);

        // Place order
        var txId = await PlaceOrderViaHttp(buyer.Id, client);

        // Confirm reservation (simulates webhook)
        await using var confirmDb = _fixture.CreateDbContext();
        var svc = new StockReservationService(confirmDb);
        await svc.ConfirmReservation(txId);

        // Verify final state
        await using var verifyDb = _fixture.CreateDbContext();
        var p = await verifyDb.Products.FindAsync(product.Id);
        p!.CountInStock.Should().Be(47); // 50 - 3
        p.ReservedCount.Should().Be(0);

        var order = await verifyDb.Orders.FirstAsync(o => o.TransactionId == txId);
        order.Status.Should().Be(OrderStatus.InTransit);
    }

    [Fact]
    public async Task PlaceOrder_Expire_StockReleasedOrdersCancelled()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id, stock: 50);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, product.Id, count: 5);

        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        var txId = await PlaceOrderViaHttp(buyer.Id, client);

        // Expire (simulates webhook)
        await using var expireDb = _fixture.CreateDbContext();
        var svc = new StockReservationService(expireDb);
        await svc.ReleaseReservation(txId);

        // Verify
        await using var verifyDb = _fixture.CreateDbContext();
        var p = await verifyDb.Products.FindAsync(product.Id);
        p!.CountInStock.Should().Be(50); // unchanged
        p.ReservedCount.Should().Be(0);  // released

        var order = await verifyDb.Orders.FirstAsync(o => o.TransactionId == txId);
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task PlaceOrder_ConfirmTwice_IdempotentNoDoubleSubtraction()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id, stock: 50);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, product.Id, count: 3);

        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        var txId = await PlaceOrderViaHttp(buyer.Id, client);

        // Confirm twice
        await using var db1 = _fixture.CreateDbContext();
        await new StockReservationService(db1).ConfirmReservation(txId);

        await using var db2 = _fixture.CreateDbContext();
        await new StockReservationService(db2).ConfirmReservation(txId);

        // Verify stock subtracted only once
        await using var verifyDb = _fixture.CreateDbContext();
        var p = await verifyDb.Products.FindAsync(product.Id);
        p!.CountInStock.Should().Be(47); // 50 - 3, not 50 - 6
    }

    [Fact]
    public async Task PlaceOrder_ExpireTwice_IdempotentNoDoubleRelease()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id, stock: 50);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, product.Id, count: 5);

        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        var txId = await PlaceOrderViaHttp(buyer.Id, client);

        // Expire twice
        await using var db1 = _fixture.CreateDbContext();
        await new StockReservationService(db1).ReleaseReservation(txId);

        await using var db2 = _fixture.CreateDbContext();
        await new StockReservationService(db2).ReleaseReservation(txId);

        // No double release, no exception
        await using var verifyDb = _fixture.CreateDbContext();
        var p = await verifyDb.Products.FindAsync(product.Id);
        p!.ReservedCount.Should().Be(0);
    }

    [Fact]
    public async Task PlaceOrder_MultipleProducts_AllReservedAllConfirmed()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var p1 = TestDatabaseHelper.SeedProduct(db, seller.Id, stock: 100);
        var p2 = TestDatabaseHelper.SeedProduct(db, seller.Id, stock: 50);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, p1.Id, count: 3);
        TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, p2.Id, count: 7);

        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        var txId = await PlaceOrderViaHttp(buyer.Id, client);

        // Confirm
        await using var confirmDb = _fixture.CreateDbContext();
        await new StockReservationService(confirmDb).ConfirmReservation(txId);

        // Verify both products
        await using var verifyDb = _fixture.CreateDbContext();
        (await verifyDb.Products.FindAsync(p1.Id))!.CountInStock.Should().Be(97);  // 100 - 3
        (await verifyDb.Products.FindAsync(p2.Id))!.CountInStock.Should().Be(43);  // 50 - 7

        var orders = await verifyDb.Orders
            .Where(o => o.TransactionId == txId)
            .ToListAsync();
        orders.Should().HaveCount(2);
        orders.Should().AllSatisfy(o => o.Status.Should().Be(OrderStatus.InTransit));
    }
}
