using ECommerce.Tests.Helpers;
using FluentAssertions;

namespace ECommerce.Tests.Domain;

public class ProductTests
{
    // #21
    [Fact]
    public void AvailableStock_ReturnsStockMinusReserved()
    {
        var product = TestData.CreateProduct(countInStock: 10, reservedCount: 3);

        product.AvailableStock.Should().Be(7);
    }

    // #22
    [Fact]
    public void AvailableStock_ReturnsZero_WhenFullyReserved()
    {
        var product = TestData.CreateProduct(countInStock: 5, reservedCount: 5);

        product.AvailableStock.Should().Be(0);
    }

    // #23
    [Fact]
    public void AvailableStock_ReturnsCountInStock_WhenNoReservations()
    {
        var product = TestData.CreateProduct(countInStock: 10, reservedCount: 0);

        product.AvailableStock.Should().Be(10);
    }
}
