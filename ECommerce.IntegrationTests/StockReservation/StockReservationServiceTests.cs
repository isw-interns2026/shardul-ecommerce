using ECommerce.Data;
using ECommerce.IntegrationTests.Fixtures;
using ECommerce.Models.Domain.Entities;
using ECommerce.Models.Domain.Exceptions;
using ECommerce.Services.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.IntegrationTests.StockReservation;

[Collection("Database")]
public class StockReservationServiceTests
{
    private readonly PostgresFixture _fixture;

    public StockReservationServiceTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    // ── ReserveStockForCartItems ─────────────────────────────

    [Fact]
    public async Task ReserveStock_SingleItem_SufficientStock_IncrementsReservedCount()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id, stock: 50);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        var cartItem = TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, product.Id, count: 5);
        db.Entry(cartItem).Reference(ci => ci.Product).Load();

        var svc = new StockReservationService(db);

        await svc.ReserveStockForCartItems([cartItem]);

        var updated = await db.Products.FindAsync(product.Id);
        updated!.ReservedCount.Should().Be(5);
    }

    [Fact]
    public async Task ReserveStock_MultipleItems_AllReservedCountsIncrease()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var p1 = TestDatabaseHelper.SeedProduct(db, seller.Id, stock: 100);
        var p2 = TestDatabaseHelper.SeedProduct(db, seller.Id, stock: 100);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        var ci1 = TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, p1.Id, count: 3);
        var ci2 = TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, p2.Id, count: 7);
        db.Entry(ci1).Reference(c => c.Product).Load();
        db.Entry(ci2).Reference(c => c.Product).Load();

        var svc = new StockReservationService(db);

        await svc.ReserveStockForCartItems([ci1, ci2]);

        (await db.Products.FindAsync(p1.Id))!.ReservedCount.Should().Be(3);
        (await db.Products.FindAsync(p2.Id))!.ReservedCount.Should().Be(7);
    }

    [Fact]
    public async Task ReserveStock_ProductNotFound_ThrowsProductNotFoundException()
    {
        await using var db = _fixture.CreateDbContext();
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        var fakeItem = new CartItem
        {
            CartId = buyer.Cart.Id,
            ProductId = Guid.NewGuid(), // non-existent
            Count = 1
        };

        var svc = new StockReservationService(db);

        await svc.Invoking(s => s.ReserveStockForCartItems([fakeItem]))
            .Should().ThrowAsync<ProductNotFoundException>();
    }

    [Fact]
    public async Task ReserveStock_InsufficientStock_ThrowsInsufficientStockException()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id, stock: 5);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        var cartItem = TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, product.Id, count: 10);
        db.Entry(cartItem).Reference(ci => ci.Product).Load();

        var svc = new StockReservationService(db);

        await svc.Invoking(s => s.ReserveStockForCartItems([cartItem]))
            .Should().ThrowAsync<InsufficientStockException>();
    }

    [Fact]
    public async Task ReserveStock_ExactlyAvailableStock_Succeeds()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id, stock: 10, reserved: 3);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        var cartItem = TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, product.Id, count: 7);
        db.Entry(cartItem).Reference(ci => ci.Product).Load();

        var svc = new StockReservationService(db);

        await svc.ReserveStockForCartItems([cartItem]);

        var updated = await db.Products.FindAsync(product.Id);
        updated!.ReservedCount.Should().Be(10); // 3 existing + 7 new
    }

    [Fact]
    public async Task ReserveStock_PartiallyReserved_ExceedsRemaining_Throws()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id, stock: 10, reserved: 8);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        var cartItem = TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, product.Id, count: 5);
        db.Entry(cartItem).Reference(ci => ci.Product).Load();

        var svc = new StockReservationService(db);

        await svc.Invoking(s => s.ReserveStockForCartItems([cartItem]))
            .Should().ThrowAsync<InsufficientStockException>();
    }

    [Fact]
    public async Task ReserveStock_ConcurrentModification_RetriesSuccessfully()
    {
        // Seed product in a separate context
        await using var seedDb = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(seedDb);
        var product = TestDatabaseHelper.SeedProduct(seedDb, seller.Id, stock: 100);
        var buyer = TestDatabaseHelper.SeedBuyer(seedDb);
        var cartItem = TestDatabaseHelper.SeedCartItem(seedDb, buyer.Cart.Id, product.Id, count: 5);

        // Context 1: will try to reserve
        await using var db1 = _fixture.CreateDbContext();
        var ci1 = await db1.CartItems.Include(c => c.Product).FirstAsync(c => c.Id == cartItem.Id);

        // Context 2: simulate concurrent modification (another reservation)
        await using var db2 = _fixture.CreateDbContext();
        var p2 = await db2.Products.FindAsync(product.Id);
        p2!.ReservedCount = 10; // someone else reserved 10
        await db2.SaveChangesAsync();

        // Context 1 should get a concurrency exception on first try, then retry
        var svc = new StockReservationService(db1);
        await svc.ReserveStockForCartItems([ci1]);

        // Verify in a fresh context
        await using var verifyDb = _fixture.CreateDbContext();
        var final = await verifyDb.Products.FindAsync(product.Id);
        final!.ReservedCount.Should().Be(15); // 10 (concurrent) + 5 (our reservation)
    }

    // ── ConfirmReservation ───────────────────────────────────

    [Fact]
    public async Task ConfirmReservation_Processing_SubtractsStockAndMarksInTransit()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = TestDatabaseHelper.SeedFullCheckoutScenario(db, productStock: 50, cartItemCount: 3);

        var svc = new StockReservationService(db);
        await svc.ConfirmReservation(scenario.Transaction.Id);

        await using var verifyDb = _fixture.CreateDbContext();
        var product = await verifyDb.Products.FindAsync(scenario.Product.Id);
        product!.CountInStock.Should().Be(47); // 50 - 3
        product.ReservedCount.Should().Be(0);  // 3 - 3

        var order = await verifyDb.Orders.FirstAsync(o => o.TransactionId == scenario.Transaction.Id);
        order.Status.Should().Be(OrderStatus.InTransit);

        var tx = await verifyDb.Transactions.FindAsync(scenario.Transaction.Id);
        tx!.Status.Should().Be(TransactionStatus.Success);
    }

    [Fact]
    public async Task ConfirmReservation_AlreadySuccess_NoChanges()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = TestDatabaseHelper.SeedFullCheckoutScenario(db);

        // Confirm once
        var svc = new StockReservationService(db);
        await svc.ConfirmReservation(scenario.Transaction.Id);

        var stockAfterFirst = (await db.Products.FindAsync(scenario.Product.Id))!.CountInStock;

        // Confirm again — should be no-op
        await using var db2 = _fixture.CreateDbContext();
        var svc2 = new StockReservationService(db2);
        await svc2.ConfirmReservation(scenario.Transaction.Id);

        await using var verifyDb = _fixture.CreateDbContext();
        var product = await verifyDb.Products.FindAsync(scenario.Product.Id);
        product!.CountInStock.Should().Be(stockAfterFirst);
    }

    [Fact]
    public async Task ConfirmReservation_NonExistentId_NoChanges()
    {
        await using var db = _fixture.CreateDbContext();
        var svc = new StockReservationService(db);

        // Should not throw — just returns
        await svc.ConfirmReservation(Guid.NewGuid());
    }

    [Fact]
    public async Task ConfirmReservation_MultipleOrders_AllUpdated()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var p1 = TestDatabaseHelper.SeedProduct(db, seller.Id, stock: 50);
        var p2 = TestDatabaseHelper.SeedProduct(db, seller.Id, stock: 30);
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        var (tx, orders) = TestDatabaseHelper.SeedTransactionWithOrders(db, buyer,
            [(p1, 5), (p2, 3)]);

        p1.ReservedCount = 5;
        p2.ReservedCount = 3;
        db.SaveChanges();

        var svc = new StockReservationService(db);
        await svc.ConfirmReservation(tx.Id);

        await using var verifyDb = _fixture.CreateDbContext();
        (await verifyDb.Products.FindAsync(p1.Id))!.CountInStock.Should().Be(45);
        (await verifyDb.Products.FindAsync(p2.Id))!.CountInStock.Should().Be(27);
    }

    // ── ReleaseReservation ───────────────────────────────────

    [Fact]
    public async Task ReleaseReservation_Processing_DecreasesReservedAndCancelsOrders()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = TestDatabaseHelper.SeedFullCheckoutScenario(db, productStock: 50, cartItemCount: 3);

        var svc = new StockReservationService(db);
        await svc.ReleaseReservation(scenario.Transaction.Id);

        await using var verifyDb = _fixture.CreateDbContext();
        var product = await verifyDb.Products.FindAsync(scenario.Product.Id);
        product!.CountInStock.Should().Be(50); // unchanged
        product.ReservedCount.Should().Be(0);  // 3 - 3

        var order = await verifyDb.Orders.FirstAsync(o => o.TransactionId == scenario.Transaction.Id);
        order.Status.Should().Be(OrderStatus.Cancelled);

        var tx = await verifyDb.Transactions.FindAsync(scenario.Transaction.Id);
        tx!.Status.Should().Be(TransactionStatus.Expired);
    }

    [Fact]
    public async Task ReleaseReservation_AlreadyExpired_NoChanges()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = TestDatabaseHelper.SeedFullCheckoutScenario(db);

        var svc = new StockReservationService(db);
        await svc.ReleaseReservation(scenario.Transaction.Id);

        // Release again
        await using var db2 = _fixture.CreateDbContext();
        var svc2 = new StockReservationService(db2);
        await svc2.ReleaseReservation(scenario.Transaction.Id);

        // No exception, idempotent
    }

    [Fact]
    public async Task ReleaseReservation_NonExistentId_NoChanges()
    {
        await using var db = _fixture.CreateDbContext();
        var svc = new StockReservationService(db);
        await svc.ReleaseReservation(Guid.NewGuid());
    }

    [Fact]
    public async Task ReleaseReservation_CountInStockUnchanged()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = TestDatabaseHelper.SeedFullCheckoutScenario(db, productStock: 50, cartItemCount: 3);

        var svc = new StockReservationService(db);
        await svc.ReleaseReservation(scenario.Transaction.Id);

        await using var verifyDb = _fixture.CreateDbContext();
        var product = await verifyDb.Products.FindAsync(scenario.Product.Id);
        product!.CountInStock.Should().Be(50); // Release never touches CountInStock
    }
}
