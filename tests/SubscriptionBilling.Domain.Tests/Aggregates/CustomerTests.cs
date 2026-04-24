using SubscriptionBilling.Domain.Abstractions;
using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Domain.Tests.Aggregates;

public sealed class CustomerTests
{
    [Fact]
    public void Creating_Customer_With_Valid_Data_Succeeds()
    {
        var name = "John Doe";
        var email = "john@example.com";
        var now = DateTime.UtcNow;

        var customer = Customer.Create(name, email, now);

        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal(name, customer.Name);
        Assert.Equal(email.ToLowerInvariant(), customer.Email.Value);
        Assert.Equal(now, customer.CreatedOnUtc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Creating_Customer_With_Empty_Name_Throws_DomainException(string? emptyName)
    {
        var exception = Assert.Throws<DomainException>(() => Customer.Create(emptyName!, "test@example.com", DateTime.UtcNow));

        Assert.Equal("Customer name is required.", exception.Message);
    }

    [Fact]
    public void Creating_Customer_With_Invalid_Email_Throws_DomainException()
    {
        var exception = Assert.Throws<DomainException>(() => Customer.Create("John Doe", "invalid-email", DateTime.UtcNow));

        Assert.Equal("Email address is invalid.", exception.Message);
    }

    [Fact]
    public void Creating_Customer_Trims_Name()
    {
        var customer = Customer.Create("  John Doe  ", "john@example.com", DateTime.UtcNow);

        Assert.Equal("John Doe", customer.Name);
    }

    [Fact]
    public void Creating_Customer_Generates_Unique_Ids()
    {
        var customer1 = Customer.Create("John Doe", "john@example.com", DateTime.UtcNow);
        var customer2 = Customer.Create("Jane Doe", "jane@example.com", DateTime.UtcNow);

        Assert.NotEqual(customer1.Id, customer2.Id);
    }

    [Fact]
    public void Created_Customer_Has_Email_Value_Object()
    {
        var customer = Customer.Create("John Doe", "john@example.com", DateTime.UtcNow);

        Assert.NotNull(customer.Email);
        Assert.IsType<EmailAddress>(customer.Email);
    }

    [Fact]
    public void Creating_Customer_Raises_No_Domain_Events_Currently()
    {
        // Note: Based on the current implementation, Customer.Create raises no domain events
        // This test documents current behavior and can be updated if behavior changes
        var customer = Customer.Create("John Doe", "john@example.com", DateTime.UtcNow);

        Assert.Empty(customer.DomainEvents);
    }

    [Fact]
    public void Customer_Email_Is_Lowercase()
    {
        var customer = Customer.Create("John Doe", "John@Example.COM", DateTime.UtcNow);

        Assert.Equal("john@example.com", customer.Email.Value);
    }

    [Fact]
    public void Customer_CreatedOnUtc_Preserves_Exact_DateTime()
    {
        var now = new DateTime(2026, 4, 21, 10, 30, 45, DateTimeKind.Utc);

        var customer = Customer.Create("John Doe", "john@example.com", now);

        Assert.Equal(now, customer.CreatedOnUtc);
    }
}
