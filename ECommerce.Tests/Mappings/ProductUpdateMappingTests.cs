using ECommerce.Mappings;
using ECommerce.Models.DTO.Seller;
using ECommerce.Tests.Helpers;
using FluentAssertions;

namespace ECommerce.Tests.Mappings;

public class ProductUpdateMappingTests
{
    // #111
    [Fact]
    public void ApplyUpdate_NonNullSkuApplied_NullNameUnchanged()
    {
        var product = TestData.CreateProduct(sku: "OLD-SKU", name: "Old Name");
        var dto = new UpdateProductDto { Sku = "NEW-SKU", Name = null };

        ProductUpdateMapper.ApplyUpdate(dto, product);

        product.Sku.Should().Be("NEW-SKU");
        product.Name.Should().Be("Old Name");
    }

    // #112
    [Fact]
    public void ApplyUpdate_NonNullPriceApplied_NullDescriptionUnchanged()
    {
        var product = TestData.CreateProduct(price: 10m, description: "Original");
        var dto = new UpdateProductDto { Price = 25m, Description = null };

        ProductUpdateMapper.ApplyUpdate(dto, product);

        product.Price.Should().Be(25m);
        product.Description.Should().Be("Original");
    }

    // #113
    [Fact]
    public void ApplyUpdate_AllNonNullFieldsApplied()
    {
        var product = TestData.CreateProduct(
            sku: "OLD", name: "Old", price: 1m,
            description: "Old desc", imageUrl: "https://old.com/img.png", isListed: false);

        var dto = new UpdateProductDto
        {
            Sku = "NEW",
            Name = "New",
            Price = 99m,
            Description = "New desc",
            ImageUrl = "https://new.com/img.png",
            IsListed = true
        };

        ProductUpdateMapper.ApplyUpdate(dto, product);

        product.Sku.Should().Be("NEW");
        product.Name.Should().Be("New");
        product.Price.Should().Be(99m);
        product.Description.Should().Be("New desc");
        product.ImageUrl.Should().Be("https://new.com/img.png");
        product.IsListed.Should().BeTrue();
    }

    // #114
    [Fact]
    public void ApplyUpdate_AllNullFieldsLeaveTargetUnchanged()
    {
        var product = TestData.CreateProduct(
            sku: "KEEP", name: "Keep", price: 50m,
            description: "Keep desc", imageUrl: "https://keep.com/img.png", isListed: true);

        var dto = new UpdateProductDto(); // all nulls

        ProductUpdateMapper.ApplyUpdate(dto, product);

        product.Sku.Should().Be("KEEP");
        product.Name.Should().Be("Keep");
        product.Price.Should().Be(50m);
        product.Description.Should().Be("Keep desc");
        product.ImageUrl.Should().Be("https://keep.com/img.png");
        product.IsListed.Should().BeTrue();
    }

    // #115
    [Fact]
    public void ApplyUpdate_IsListedNonNullApplied()
    {
        var product = TestData.CreateProduct(isListed: true);
        var dto = new UpdateProductDto { IsListed = false };

        ProductUpdateMapper.ApplyUpdate(dto, product);

        product.IsListed.Should().BeFalse();
    }
}
