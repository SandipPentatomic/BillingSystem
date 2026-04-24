using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Domain.Tests;

public sealed class SubscriptionTests
{
    [Fact]
    public void Creating_Subscription_Generates_Initial_Invoice_Draft()
    {
        var now = new DateTime(2026, 4, 21, 10, 0, 0, DateTimeKind.Utc);
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "Pro Plan",
            new Money(49.99m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            now);

        var draft = subscription.GenerateInitialInvoice(now);

        Assert.Equal(subscription.Id, draft.SubscriptionId);
        Assert.Equal(subscription.CustomerId, draft.CustomerId);
        Assert.Equal(new DateTime(2026, 4, 21, 10, 0, 0, DateTimeKind.Utc), draft.PeriodStartUtc);
        Assert.Equal(new DateTime(2026, 5, 21, 10, 0, 0, DateTimeKind.Utc), draft.PeriodEndUtc);
        Assert.Equal(new DateTime(2026, 5, 21, 10, 0, 0, DateTimeKind.Utc), subscription.NextBillingDateUtc);
        Assert.True(subscription.InitialInvoiceGenerated);
    }

    [Fact]
    public void Billing_Cycle_Generates_Invoice_When_Subscription_Is_Due()
    {
        var activatedOn = new DateTime(2026, 1, 10, 9, 0, 0, DateTimeKind.Utc);
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "Team Plan",
            new Money(99m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            activatedOn);

        subscription.GenerateInitialInvoice(activatedOn);

        var dueInvoices = subscription.GenerateDueInvoices(new DateTime(2026, 2, 15, 9, 0, 0, DateTimeKind.Utc));

        var invoice = Assert.Single(dueInvoices);
        Assert.Equal(new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc), invoice.PeriodStartUtc);
        Assert.Equal(new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc), invoice.PeriodEndUtc);
        Assert.Equal(new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc), subscription.NextBillingDateUtc);
    }

    [Fact]
    public void Cancelling_Subscription_Stops_Future_Invoices()
    {
        var activatedOn = new DateTime(2026, 1, 5, 8, 0, 0, DateTimeKind.Utc);
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "Starter",
            new Money(10m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            activatedOn);

        subscription.GenerateInitialInvoice(activatedOn);
        subscription.Cancel(new DateTime(2026, 1, 20, 8, 0, 0, DateTimeKind.Utc));

        var dueInvoices = subscription.GenerateDueInvoices(new DateTime(2026, 2, 6, 8, 0, 0, DateTimeKind.Utc));

        Assert.Empty(dueInvoices);
        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
    }

    [Fact]
    public void Billing_Cycle_Supports_Minute_Based_Subscriptions_For_Testing()
    {
        var activatedOn = new DateTime(2026, 4, 21, 10, 0, 0, DateTimeKind.Utc);
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "Fast Test Plan",
            new Money(5m, "USD"),
            new BillingCycle(15, BillingIntervalUnit.Minutes),
            activatedOn);

        subscription.GenerateInitialInvoice(activatedOn);

        var dueInvoices = subscription.GenerateDueInvoices(new DateTime(2026, 4, 21, 10, 15, 0, DateTimeKind.Utc));

        var invoice = Assert.Single(dueInvoices);
        Assert.Equal(new DateTime(2026, 4, 21, 10, 15, 0, DateTimeKind.Utc), invoice.PeriodStartUtc);
        Assert.Equal(new DateTime(2026, 4, 21, 10, 30, 0, DateTimeKind.Utc), invoice.PeriodEndUtc);
        Assert.Equal(new DateTime(2026, 4, 21, 10, 30, 0, DateTimeKind.Utc), subscription.NextBillingDateUtc);
    }
}
