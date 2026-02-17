using ECommerce.Data;
using ECommerce.Models.Domain.Entities;

namespace ECommerce.IntegrationTests.Fixtures;

public static class TestDatabaseHelper
{
    public static Buyer SeedBuyer(ECommerceDbContext db, string? emailPrefix = null)
    {
        var prefix = emailPrefix ?? Guid.NewGuid().ToString("N")[..8];
        var buyer = Buyer.Create(
            name: $"Buyer_{prefix}",
            email: $"{prefix}@test.com",
            passwordHash: BCrypt.Net.BCrypt.EnhancedHashPassword("Test1234!"),
            address: "123 Test Street"
        );
        db.Add(buyer);
        db.SaveChanges();
        return buyer;
    }

    public static Seller SeedSeller(ECommerceDbContext db, string? emailPrefix = null)
    {
        var prefix = emailPrefix ?? Guid.NewGuid().ToString("N")[..8];
        var seller = Seller.Create(
            name: $"Seller_{prefix}",
            email: $"{prefix}@test.com",
            passwordHash: BCrypt.Net.BCrypt.EnhancedHashPassword("Test1234!"),
            bankAccountNumber: "GB82WEST12345698765432"
        );
        db.Add(seller);
        db.SaveChanges();
        return seller;
    }

    public static Product SeedProduct(ECommerceDbContext db, Guid sellerId,
        int stock = 100, int reserved = 0, decimal price = 99.99m,
        bool isListed = true, string? sku = null)
    {
        var product = new Product
        {
            SellerId = sellerId,
            Sku = sku ?? $"SKU-{Guid.NewGuid():N}"[..20],
            Name = $"Product_{Guid.NewGuid():N}"[..20],
            Price = price,
            CountInStock = stock,
            ReservedCount = reserved,
            IsListed = isListed,
            Description = "Test product"
        };
        db.Add(product);
        db.SaveChanges();
        return product;
    }

    public static CartItem SeedCartItem(ECommerceDbContext db, Guid cartId, Guid productId, int count = 2)
    {
        var cartItem = new CartItem
        {
            CartId = cartId,
            ProductId = productId,
            Count = count
        };
        db.Add(cartItem);
        db.SaveChanges();
        return cartItem;
    }

    public static (Transaction transaction, List<Order> orders) SeedTransactionWithOrders(
        ECommerceDbContext db, Buyer buyer, List<(Product product, int count)> items,
        TransactionStatus status = TransactionStatus.Processing)
    {
        var transaction = new Transaction
        {
            Amount = items.Sum(i => i.product.Price * i.count),
            Status = status
        };
        db.Add(transaction);
        db.SaveChanges();

        var orders = new List<Order>();
        foreach (var (product, count) in items)
        {
            var cartItem = new CartItem
            {
                CartId = buyer.Cart.Id,
                ProductId = product.Id,
                Count = count,
                Product = product
            };

            var order = Order.Create(buyer, cartItem, transaction);
            db.Add(order);
            orders.Add(order);
        }
        db.SaveChanges();

        return (transaction, orders);
    }

    /// <summary>
    /// Seeds a complete checkout scenario: buyer, seller, products, cart items,
    /// transaction, and orders. Returns everything needed for testing.
    /// </summary>
    public static CheckoutScenario SeedFullCheckoutScenario(ECommerceDbContext db,
        int productStock = 100, int cartItemCount = 2)
    {
        var seller = SeedSeller(db);
        var product = SeedProduct(db, seller.Id, stock: productStock);
        var buyer = SeedBuyer(db);

        var cartItem = SeedCartItem(db, buyer.Cart.Id, product.Id, count: cartItemCount);

        // Reload cart item with Product navigation
        db.Entry(cartItem).Reference(ci => ci.Product).Load();

        var (transaction, orders) = SeedTransactionWithOrders(db, buyer,
            [(product, cartItemCount)]);

        // Simulate stock reservation
        product.ReservedCount += cartItemCount;
        db.SaveChanges();

        return new CheckoutScenario(buyer, seller, product, cartItem, transaction, orders);
    }

    public record CheckoutScenario(
        Buyer Buyer, Seller Seller, Product Product,
        CartItem CartItem, Transaction Transaction, List<Order> Orders);
}
