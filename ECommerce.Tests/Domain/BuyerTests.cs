using ECommerce.Tests.Helpers;
using FluentAssertions;

namespace ECommerce.Tests.Domain;

public class BuyerTests
{
    // #18
    [Fact]
    public void Create_SetsAllProperties()
    {
        var buyer = TestData.CreateBuyer(
            name: "Alice",
            email: "alice@test.com",
            passwordHash: "hashed123",
            address: "10 Downing St");

        buyer.Name.Should().Be("Alice");
        buyer.Email.Should().Be("alice@test.com");
        buyer.PasswordHash.Should().Be("hashed123");
        buyer.Address.Should().Be("10 Downing St");
    }

    // #19
    [Fact]
    public void Create_CartIsNotNull()
    {
        var buyer = TestData.CreateBuyer();

        buyer.Cart.Should().NotBeNull();
    }

    // #20
    [Fact]
    public void Create_CartBuyerReferencesCreatedBuyer()
    {
        var buyer = TestData.CreateBuyer();

        buyer.Cart.Buyer.Should().BeSameAs(buyer);
    }
}
