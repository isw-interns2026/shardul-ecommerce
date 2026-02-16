using ECommerce.Models.Domain.Entities;
using ECommerce.Models.Domain.Exceptions;
using ECommerce.Tests.Helpers;
using FluentAssertions;

namespace ECommerce.Tests.Domain;

public class OrderTests
{
    // ── TransitionTo ─────────────────────────────────────────

    // #1 — B3: TryGetValue succeeds, Contains succeeds
    [Fact]
    public void TransitionTo_InTransitToDelivered_Succeeds()
    {
        var order = TestData.CreateOrder();
        order.MarkInTransit();

        order.TransitionTo(OrderStatus.Delivered);

        order.Status.Should().Be(OrderStatus.Delivered);
    }

    // #2 — B1: TryGetValue fails (AwaitingPayment not in dict)
    [Fact]
    public void TransitionTo_AwaitingPaymentToDelivered_Throws()
    {
        var order = TestData.CreateOrder();
        order.Status.Should().Be(OrderStatus.AwaitingPayment);

        var act = () => order.TransitionTo(OrderStatus.Delivered);

        act.Should().Throw<InvalidOrderStatusTransitionException>();
    }

    // #3 — B1: TryGetValue fails (Cancelled not in dict)
    [Fact]
    public void TransitionTo_CancelledToInTransit_Throws()
    {
        var order = TestData.CreateOrder();
        order.MarkCancelled();

        var act = () => order.TransitionTo(OrderStatus.InTransit);

        act.Should().Throw<InvalidOrderStatusTransitionException>();
    }

    // #4 — B1: TryGetValue fails (Delivered not in dict)
    [Fact]
    public void TransitionTo_DeliveredToInTransit_Throws()
    {
        var order = TestData.CreateOrder();
        order.MarkInTransit();
        order.TransitionTo(OrderStatus.Delivered);

        var act = () => order.TransitionTo(OrderStatus.InTransit);

        act.Should().Throw<InvalidOrderStatusTransitionException>();
    }

    // #5 — B1: TryGetValue fails (AwaitingPayment not in dict)
    [Fact]
    public void TransitionTo_AwaitingPaymentToInTransit_Throws()
    {
        var order = TestData.CreateOrder();

        var act = () => order.TransitionTo(OrderStatus.InTransit);

        act.Should().Throw<InvalidOrderStatusTransitionException>();
    }

    // #6 — B2: TryGetValue succeeds (InTransit in dict), but Cancelled not in allowed set
    [Fact]
    public void TransitionTo_InTransitToCancelled_Throws()
    {
        var order = TestData.CreateOrder();
        order.MarkInTransit();

        var act = () => order.TransitionTo(OrderStatus.Cancelled);

        act.Should().Throw<InvalidOrderStatusTransitionException>();
    }

    // #7 — B2: TryGetValue succeeds (InTransit in dict), but AwaitingPayment not in allowed set
    [Fact]
    public void TransitionTo_InTransitToAwaitingPayment_Throws()
    {
        var order = TestData.CreateOrder();
        order.MarkInTransit();

        var act = () => order.TransitionTo(OrderStatus.AwaitingPayment);

        act.Should().Throw<InvalidOrderStatusTransitionException>();
    }

    // ── MarkInTransit / MarkCancelled ────────────────────────

    // #8
    [Fact]
    public void MarkInTransit_SetsStatusToInTransit()
    {
        var order = TestData.CreateOrder();

        order.MarkInTransit();

        order.Status.Should().Be(OrderStatus.InTransit);
    }

    // #9
    [Fact]
    public void MarkCancelled_SetsStatusToCancelled()
    {
        var order = TestData.CreateOrder();

        order.MarkCancelled();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    // ── Order.Create ─────────────────────────────────────────

    // #10
    [Fact]
    public void Create_SetsBuyerIdFromBuyer()
    {
        var buyer = TestData.CreateBuyer();
        var cartItem = TestData.CreateCartItem();
        var transaction = TestData.CreateTransaction();

        var order = Order.Create(buyer, cartItem, transaction);

        order.BuyerId.Should().Be(buyer.Id);
    }

    // #11
    [Fact]
    public void Create_SetsSellerIdFromProduct()
    {
        var product = TestData.CreateProduct(sellerId: Guid.NewGuid());
        var cartItem = TestData.CreateCartItem(product: product);

        var order = Order.Create(TestData.CreateBuyer(), cartItem, TestData.CreateTransaction());

        order.SellerId.Should().Be(product.SellerId);
    }

    // #12
    [Fact]
    public void Create_SetsProductIdFromCartItem()
    {
        var cartItem = TestData.CreateCartItem();

        var order = Order.Create(TestData.CreateBuyer(), cartItem, TestData.CreateTransaction());

        order.ProductId.Should().Be(cartItem.ProductId);
    }

    // #13
    [Fact]
    public void Create_ComputesTotalAsPriceTimesCount()
    {
        var product = TestData.CreateProduct(price: 25.50m);
        var cartItem = TestData.CreateCartItem(product: product, count: 4);

        var order = Order.Create(TestData.CreateBuyer(), cartItem, TestData.CreateTransaction());

        order.Total.Should().Be(25.50m * 4);
    }

    // #14
    [Fact]
    public void Create_SetsAddressFromBuyer()
    {
        var buyer = TestData.CreateBuyer(address: "789 Oak Avenue");

        var order = Order.Create(buyer, TestData.CreateCartItem(), TestData.CreateTransaction());

        order.Address.Should().Be("789 Oak Avenue");
    }

    // #15
    [Fact]
    public void Create_SetsStatusToAwaitingPayment()
    {
        var order = TestData.CreateOrder();

        order.Status.Should().Be(OrderStatus.AwaitingPayment);
    }

    // #16
    [Fact]
    public void Create_SetsTransactionReference()
    {
        var transaction = TestData.CreateTransaction();

        var order = Order.Create(TestData.CreateBuyer(), TestData.CreateCartItem(), transaction);

        order.Transaction.Should().BeSameAs(transaction);
    }

    // #17
    [Fact]
    public void Create_SetsCountFromCartItem()
    {
        var cartItem = TestData.CreateCartItem(count: 7);

        var order = Order.Create(TestData.CreateBuyer(), cartItem, TestData.CreateTransaction());

        order.Count.Should().Be(7);
    }
}
