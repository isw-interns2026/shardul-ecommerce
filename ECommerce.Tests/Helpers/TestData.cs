using ECommerce.Models.Domain.Entities;
using ECommerce.Models.DTO.Auth.Request;
using ECommerce.Models.DTO.Seller;

namespace ECommerce.Tests.Helpers;

public static class TestData
{
    public static Product CreateProduct(
        Guid? id = null,
        Guid? sellerId = null,
        string sku = "TEST-SKU",
        string name = "Test Product",
        decimal price = 99.99m,
        int countInStock = 100,
        int reservedCount = 0,
        string? description = "A test product",
        string? imageUrl = "https://example.com/image.png",
        bool isListed = true)
    {
        return new Product
        {
            Id = id ?? Guid.NewGuid(),
            SellerId = sellerId ?? Guid.NewGuid(),
            Sku = sku,
            Name = name,
            Price = price,
            CountInStock = countInStock,
            ReservedCount = reservedCount,
            Description = description,
            ImageUrl = imageUrl,
            IsListed = isListed
        };
    }

    public static Buyer CreateBuyer(
        string name = "Test Buyer",
        string email = "buyer@test.com",
        string passwordHash = "$2a$11$hashedpassword",
        string address = "123 Test Street")
    {
        return Buyer.Create(name, email, passwordHash, address);
    }

    public static CartItem CreateCartItem(Product? product = null, Guid? cartId = null, int count = 2)
    {
        product ??= CreateProduct();
        return new CartItem
        {
            CartId = cartId ?? Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            Count = count
        };
    }

    public static Transaction CreateTransaction(
        Guid? id = null,
        decimal amount = 199.98m,
        TransactionStatus status = TransactionStatus.Processing,
        string? stripeSessionId = null)
    {
        return new Transaction
        {
            Id = id ?? Guid.NewGuid(),
            Amount = amount,
            Status = status,
            StripeSessionId = stripeSessionId
        };
    }

    public static Order CreateOrder(Buyer? buyer = null, CartItem? cartItem = null, Transaction? transaction = null)
    {
        buyer ??= CreateBuyer();
        cartItem ??= CreateCartItem();
        transaction ??= CreateTransaction();
        return Order.Create(buyer, cartItem, transaction);
    }

    public static BuyerRegisterRequestDto ValidBuyerRegisterDto() => new()
    {
        Name = "John Doe",
        Email = "john@example.com",
        Password = "SecurePass123",
        Address = "456 Main Street"
    };

    public static SellerRegisterRequestDto ValidSellerRegisterDto() => new()
    {
        Name = "Jane Seller",
        Email = "jane@example.com",
        Password = "SecurePass123",
        BankAccountNumber = "GB29NWBK60161331926819"
    };

    public static LoginRequestDto ValidLoginDto() => new()
    {
        Email = "user@example.com",
        Password = "SecurePass123"
    };

    public static AddProductDto ValidAddProductDto() => new()
    {
        Sku = "PROD-001",
        Name = "Widget",
        Price = 29.99m,
        CountInStock = 50,
        Description = "A fine widget",
        ImageUrl = "https://example.com/widget.png",
        IsListed = true
    };

    public static UpdateProductDto ValidUpdateProductDto() => new()
    {
        Sku = "PROD-002",
        Name = "Updated Widget",
        Price = 39.99m,
        Description = "An updated widget",
        ImageUrl = "https://example.com/updated.png",
        IsListed = false
    };
}
