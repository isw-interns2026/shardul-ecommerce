using ECommerce.Mappings;
using ECommerce.Models.Domain.Entities;
using ECommerce.Tests.Helpers;
using FluentAssertions;

namespace ECommerce.Tests.Mappings;

public class OrderMappingTests
{
    // ── ToBuyerOrderDto ──────────────────────────────────────

    // #90 — B1: Product null → throw
    [Fact]
    public void ToBuyerOrderDto_ProductNull_ThrowsArgumentNullException()
    {
        var order = TestData.CreateOrder();
        order.Product = null;

        var act = () => order.ToBuyerOrderDto();

        act.Should().Throw<ArgumentNullException>();
    }

    // #91 — B2: Product populated → maps
    [Fact]
    public void ToBuyerOrderDto_ProductPopulated_Succeeds()
    {
        var product = TestData.CreateProduct();
        var cartItem = TestData.CreateCartItem(product: product);
        var order = Order.Create(TestData.CreateBuyer(), cartItem, TestData.CreateTransaction());
        order.Product = product;

        var dto = order.ToBuyerOrderDto();

        dto.Should().NotBeNull();
    }

    // #92
    [Fact]
    public void ToBuyerOrderDto_MapsOrderIdFromId()
    {
        var product = TestData.CreateProduct();
        var cartItem = TestData.CreateCartItem(product: product);
        var order = Order.Create(TestData.CreateBuyer(), cartItem, TestData.CreateTransaction());
        order.Product = product;

        var dto = order.ToBuyerOrderDto();

        dto.OrderId.Should().Be(order.Id);
    }

    // #93
    [Fact]
    public void ToBuyerOrderDto_MapsOrderValueFromTotal()
    {
        var product = TestData.CreateProduct(price: 50m);
        var cartItem = TestData.CreateCartItem(product: product, count: 3);
        var order = Order.Create(TestData.CreateBuyer(), cartItem, TestData.CreateTransaction());
        order.Product = product;

        var dto = order.ToBuyerOrderDto();

        dto.OrderValue.Should().Be(150m);
    }

    // #94
    [Fact]
    public void ToBuyerOrderDto_MapsProductCountFromCount()
    {
        var product = TestData.CreateProduct();
        var cartItem = TestData.CreateCartItem(product: product, count: 5);
        var order = Order.Create(TestData.CreateBuyer(), cartItem, TestData.CreateTransaction());
        order.Product = product;

        var dto = order.ToBuyerOrderDto();

        dto.ProductCount.Should().Be(5);
    }

    // #95
    [Fact]
    public void ToBuyerOrderDto_MapsDeliveryAddressFromAddress()
    {
        var product = TestData.CreateProduct();
        var cartItem = TestData.CreateCartItem(product: product);
        var buyer = TestData.CreateBuyer(address: "42 Wallaby Way");
        var order = Order.Create(buyer, cartItem, TestData.CreateTransaction());
        order.Product = product;

        var dto = order.ToBuyerOrderDto();

        dto.DeliveryAddress.Should().Be("42 Wallaby Way");
    }

    // #96
    [Fact]
    public void ToBuyerOrderDto_MapsOrderStatusFromStatus()
    {
        var product = TestData.CreateProduct();
        var cartItem = TestData.CreateCartItem(product: product);
        var order = Order.Create(TestData.CreateBuyer(), cartItem, TestData.CreateTransaction());
        order.Product = product;

        var dto = order.ToBuyerOrderDto();

        dto.OrderStatus.Should().Be(OrderStatus.AwaitingPayment);
    }

    // #97
    [Fact]
    public void ToBuyerOrderDto_MapsProductNameFromNestedProduct()
    {
        var product = TestData.CreateProduct(name: "Super Widget");
        var cartItem = TestData.CreateCartItem(product: product);
        var order = Order.Create(TestData.CreateBuyer(), cartItem, TestData.CreateTransaction());
        order.Product = product;

        var dto = order.ToBuyerOrderDto();

        dto.ProductName.Should().Be("Super Widget");
    }

    // #98
    [Fact]
    public void ToBuyerOrderDto_MapsProductSkuFromNestedProduct()
    {
        var product = TestData.CreateProduct(sku: "WDG-001");
        var cartItem = TestData.CreateCartItem(product: product);
        var order = Order.Create(TestData.CreateBuyer(), cartItem, TestData.CreateTransaction());
        order.Product = product;

        var dto = order.ToBuyerOrderDto();

        dto.ProductSku.Should().Be("WDG-001");
    }

    // ── ToSellerOrderDto ─────────────────────────────────────

    // #99 — B1: Product null → throw
    [Fact]
    public void ToSellerOrderDto_ProductNull_ThrowsArgumentNullException()
    {
        var order = TestData.CreateOrder();
        order.Product = null;

        var act = () => order.ToSellerOrderDto();

        act.Should().Throw<ArgumentNullException>();
    }

    // #100 — B2: Product populated → maps
    [Fact]
    public void ToSellerOrderDto_ProductPopulated_MapsAllFields()
    {
        var product = TestData.CreateProduct(name: "Gadget", sku: "GDG-01");
        var cartItem = TestData.CreateCartItem(product: product, count: 2);
        var buyer = TestData.CreateBuyer(address: "99 Seller Lane");
        var order = Order.Create(buyer, cartItem, TestData.CreateTransaction());
        order.Product = product;

        var dto = order.ToSellerOrderDto();

        dto.OrderId.Should().Be(order.Id);
        dto.OrderValue.Should().Be(order.Total);
        dto.ProductCount.Should().Be(2);
        dto.DeliveryAddress.Should().Be("99 Seller Lane");
        dto.ProductName.Should().Be("Gadget");
        dto.ProductSku.Should().Be("GDG-01");
    }

    // #101
    [Fact]
    public void ToSellerOrderDto_DoesNotContainStatusProperty()
    {
        // SellerOrderResponseDto has no Status/OrderStatus property.
        // This is a compile-time guarantee — if someone adds it, this test
        // reminds them it was intentionally omitted.
        var properties = typeof(ECommerce.Models.DTO.Seller.SellerOrderResponseDto).GetProperties();

        properties.Should().NotContain(p => p.Name == "Status" || p.Name == "OrderStatus");
    }
}
