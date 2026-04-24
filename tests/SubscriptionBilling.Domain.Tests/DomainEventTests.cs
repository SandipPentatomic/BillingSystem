using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Billing;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.Events;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Domain.Tests;

public sealed class DomainEventTests
{
    [Fact]
    public void Creating_Subscription_Raises_SubscriptionActivated_Event()
    {
        var now = new DateTime(2026, 4, 21, 10, 0, 0, DateTimeKind.Utc);

        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            now);

        var domainEvent = Assert.Single(subscription.DomainEvents);
        var activatedEvent = Assert.IsType<SubscriptionActivatedDomainEvent>(domainEvent);

        Assert.Equal(subscription.Id, activatedEvent.SubscriptionId);
        Assert.Equal(subscription.CustomerId, activatedEvent.CustomerId);
    }

    [Fact]
    public void Paying_Invoice_Raises_PaymentReceived_Event()
    {
        var invoice = Invoice.Generate(new InvoiceGenerationDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Money(25m, "USD"),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 7, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)));

        invoice.ClearDomainEvents();
        invoice.MarkAsPaid(new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc), PaymentMode.Check);

        var domainEvent = Assert.Single(invoice.DomainEvents);
        var paymentReceived = Assert.IsType<PaymentReceivedDomainEvent>(domainEvent);

        Assert.Equal(invoice.Id, paymentReceived.InvoiceId);
        Assert.Equal(invoice.SubscriptionId, paymentReceived.SubscriptionId);
        Assert.Equal(PaymentMode.Check, paymentReceived.PaymentMode);
    }
}
