using ECommerce.Mappings;
using ECommerce.Tests.Helpers;
using FluentAssertions;

namespace ECommerce.Tests.Mappings;

public class ProductMappingTests
{
    // ── ToBuyerProductDto ────────────────────────────────────

    // #102
    [Fact]
    public void ToBuyerProductDto_MapsKeyFields()
    {
        var product = TestData.CreateProduct(
            sku: "SKU-1",
            name: "Widget",
            price: 19.99m,
            countInStock: 10,
            reservedCount: 3);

        var dto = product.ToBuyerProductDto();

        dto.Id.Should().Be(product.Id);
        dto.Sku.Should().Be("SKU-1");
        dto.Name.Should().Be("Widget");
        dto.Price.Should().Be(19.99m);
        dto.AvailableStock.Should().Be(7);
    }

    // #103
    [Fact]
    public void ToBuyerProductDto_MapsNullableFields()
    {
        var product = TestData.CreateProduct(description: "A desc", imageUrl: "https://img.com/a.png");

        var dto = product.ToBuyerProductDto();

        dto.Description.Should().Be("A desc");
        dto.ImageUrl.Should().Be("https://img.com/a.png");
    }

    // #104
    [Fact]
    public void ToSellerProductDto_MapsSellerSpecificFields()
    {
        var product = TestData.CreateProduct(countInStock: 25, isListed: true);

        var dto = product.ToSellerProductDto();

        dto.CountInStock.Should().Be(25);
        dto.IsListed.Should().BeTrue();
    }

    // #105
    [Fact]
    public void ToBuyerCartItemDto_CountInCartStaysZero()
    {
        var product = TestData.CreateProduct();

        var dto = product.ToBuyerCartItemDto();

        dto.CountInCart.Should().Be(0);
    }

    // #106
    [Fact]
    public void ToBuyerCartItemDto_MapsSameFieldsAsBuyerProductDto()
    {
        var product = TestData.CreateProduct(
            sku: "CART-SKU",
            name: "Cart Product",
            price: 15m,
            countInStock: 20,
            reservedCount: 5,
            description: "Cart desc",
            imageUrl: "https://img.com/cart.png");

        var dto = product.ToBuyerCartItemDto();

        dto.Id.Should().Be(product.Id);
        dto.Sku.Should().Be("CART-SKU");
        dto.Name.Should().Be("Cart Product");
        dto.Price.Should().Be(15m);
        dto.AvailableStock.Should().Be(15);
        dto.Description.Should().Be("Cart desc");
        dto.ImageUrl.Should().Be("https://img.com/cart.png");
    }

    // ── ToProduct (from AddProductDto) ───────────────────────

    // #107
    [Fact]
    public void ToProduct_MapsAllDtoFields()
    {
        var dto = TestData.ValidAddProductDto();

        var product = dto.ToProduct();

        product.Sku.Should().Be(dto.Sku);
        product.Name.Should().Be(dto.Name);
        product.Price.Should().Be(dto.Price);
        product.CountInStock.Should().Be(dto.CountInStock);
        product.Description.Should().Be(dto.Description);
        product.ImageUrl.Should().Be(dto.ImageUrl);
        product.IsListed.Should().Be(dto.IsListed);
    }

    // #108
    [Fact]
    public void ToProduct_IdStaysDefault()
    {
        var dto = TestData.ValidAddProductDto();

        var product = dto.ToProduct();

        product.Id.Should().Be(Guid.Empty);
    }

    // #109
    [Fact]
    public void ToProduct_SellerIdStaysDefault()
    {
        var dto = TestData.ValidAddProductDto();

        var product = dto.ToProduct();

        product.SellerId.Should().Be(Guid.Empty);
    }

    // #110
    [Fact]
    public void ToProduct_ReservedCountStaysZero()
    {
        var dto = TestData.ValidAddProductDto();

        var product = dto.ToProduct();

        product.ReservedCount.Should().Be(0);
    }
}
