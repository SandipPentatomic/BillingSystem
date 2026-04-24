using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Abstractions;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.Events;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Domain.Tests.Abstractions;

public sealed class AggregateRootTests
{
    [Fact]
    public void Initially_Has_No_Domain_Events()
    {
        var customer = Customer.Create("John Doe", "john@example.com", DateTime.UtcNow);

        Assert.Empty(customer.DomainEvents);

        customer.ClearDomainEvents();

        Assert.Empty(customer.DomainEvents);
    }

    [Fact]
    public void Clear_Domain_Events_Removes_All_Events()
    {
        var customer = Customer.Create("John Doe", "john@example.com", DateTime.UtcNow);
        var subscription = Subscription.Create(
            customer.Id,
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            DateTime.UtcNow);

        customer.ClearDomainEvents();
        subscription.ClearDomainEvents();

        Assert.Empty(customer.DomainEvents);
        Assert.Empty(subscription.DomainEvents);
    }
}
