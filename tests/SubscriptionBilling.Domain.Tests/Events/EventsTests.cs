using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Billing;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.Events;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Domain.Tests.Events;

public sealed class EventsTests
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

    [Fact]
    public void SubscriptionActivatedDomainEvent_Sets_All_Properties()
    {
        var subscriptionId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var occurredOn = new DateTime(2026, 4, 21, 10, 0, 0, DateTimeKind.Utc);

        var @event = new SubscriptionActivatedDomainEvent(subscriptionId, customerId, occurredOn);

        Assert.Equal(subscriptionId, @event.SubscriptionId);
        Assert.Equal(customerId, @event.CustomerId);
        Assert.Equal(occurredOn, @event.OccurredOnUtc);
    }

    [Fact]
    public void InvoiceGeneratedDomainEvent_Sets_All_Properties()
    {
        var invoiceId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var amount = 100m;
        var currency = "USD";
        var occurredOn = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        var @event = new InvoiceGeneratedDomainEvent(
            invoiceId,
            subscriptionId,
            customerId,
            amount,
            currency,
            occurredOn);

        Assert.Equal(invoiceId, @event.InvoiceId);
        Assert.Equal(subscriptionId, @event.SubscriptionId);
        Assert.Equal(customerId, @event.CustomerId);
        Assert.Equal(amount, @event.Amount);
        Assert.Equal(currency, @event.Currency);
        Assert.Equal(occurredOn, @event.OccurredOnUtc);
    }

    [Fact]
    public void PaymentReceivedDomainEvent_Sets_All_Properties()
    {
        var invoiceId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var amount = 100m;
        var currency = "USD";
        var paymentMode = PaymentMode.Online;
        var occurredOn = new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc);

        var @event = new PaymentReceivedDomainEvent(
            invoiceId,
            subscriptionId,
            customerId,
            amount,
            currency,
            paymentMode,
            occurredOn);

        Assert.Equal(invoiceId, @event.InvoiceId);
        Assert.Equal(subscriptionId, @event.SubscriptionId);
        Assert.Equal(customerId, @event.CustomerId);
        Assert.Equal(amount, @event.Amount);
        Assert.Equal(currency, @event.Currency);
        Assert.Equal(paymentMode, @event.PaymentMode);
        Assert.Equal(occurredOn, @event.OccurredOnUtc);
    }

    [Fact]
    public void Two_SubscriptionActivatedDomainEvent_Records_With_Same_Values_Are_Equal()
    {
        var subscriptionId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var occurredOn = new DateTime(2026, 4, 21, 10, 0, 0, DateTimeKind.Utc);

        var event1 = new SubscriptionActivatedDomainEvent(subscriptionId, customerId, occurredOn);
        var event2 = new SubscriptionActivatedDomainEvent(subscriptionId, customerId, occurredOn);

        Assert.Equal(event1, event2);
    }

    [Fact]
    public void Two_InvoiceGeneratedDomainEvent_Records_With_Same_Values_Are_Equal()
    {
        var invoiceId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var amount = 100m;
        var currency = "USD";
        var occurredOn = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        var event1 = new InvoiceGeneratedDomainEvent(invoiceId, subscriptionId, customerId, amount, currency, occurredOn);
        var event2 = new InvoiceGeneratedDomainEvent(invoiceId, subscriptionId, customerId, amount, currency, occurredOn);

        Assert.Equal(event1, event2);
    }

    [Fact]
    public void Two_PaymentReceivedDomainEvent_Records_With_Same_Values_Are_Equal()
    {
        var invoiceId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var amount = 100m;
        var currency = "USD";
        var paymentMode = PaymentMode.Online;
        var occurredOn = new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc);

        var event1 = new PaymentReceivedDomainEvent(invoiceId, subscriptionId, customerId, amount, currency, paymentMode, occurredOn);
        var event2 = new PaymentReceivedDomainEvent(invoiceId, subscriptionId, customerId, amount, currency, paymentMode, occurredOn);

        Assert.Equal(event1, event2);
    }

    [Fact]
    public void SubscriptionActivatedDomainEvent_Is_Record_Type()
    {
        var subscriptionId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var occurredOn = new DateTime(2026, 4, 21, 10, 0, 0, DateTimeKind.Utc);

        var @event = new SubscriptionActivatedDomainEvent(subscriptionId, customerId, occurredOn);
        var eventString = @event.ToString();

        Assert.NotNull(eventString);
        Assert.Contains(nameof(SubscriptionActivatedDomainEvent), eventString);
    }

    [Fact]
    public void InvoiceGeneratedDomainEvent_Is_Record_Type()
    {
        var invoiceId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var amount = 100m;
        var currency = "USD";
        var occurredOn = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        var @event = new InvoiceGeneratedDomainEvent(invoiceId, subscriptionId, customerId, amount, currency, occurredOn);
        var eventString = @event.ToString();

        Assert.NotNull(eventString);
        Assert.Contains(nameof(InvoiceGeneratedDomainEvent), eventString);
    }

    [Fact]
    public void PaymentReceivedDomainEvent_Is_Record_Type()
    {
        var invoiceId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var amount = 100m;
        var currency = "USD";
        var paymentMode = PaymentMode.Online;
        var occurredOn = new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc);

        var @event = new PaymentReceivedDomainEvent(invoiceId, subscriptionId, customerId, amount, currency, paymentMode, occurredOn);
        var eventString = @event.ToString();

        Assert.NotNull(eventString);
        Assert.Contains(nameof(PaymentReceivedDomainEvent), eventString);
    }
}
