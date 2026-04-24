using SubscriptionBilling.Domain.Abstractions;
using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Billing;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.Events;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Domain.Tests.Aggregates;

public sealed class InvoiceTests
{
    [Fact]
    public void Generating_Invoice_From_Draft_Succeeds()
    {
        var draft = new InvoiceGenerationDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Money(100m, "USD"),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 7, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));

        var invoice = Invoice.Generate(draft);

        Assert.Equal(draft.InvoiceId, invoice.Id);
        Assert.Equal(draft.CustomerId, invoice.CustomerId);
        Assert.Equal(draft.SubscriptionId, invoice.SubscriptionId);
        Assert.Equal(draft.Amount, invoice.Amount);
        Assert.Equal(draft.PeriodStartUtc, invoice.PeriodStartUtc);
        Assert.Equal(draft.PeriodEndUtc, invoice.PeriodEndUtc);
        Assert.Equal(draft.DueDateUtc, invoice.DueDateUtc);
        Assert.Equal(draft.IssuedOnUtc, invoice.IssuedOnUtc);
        Assert.Equal(InvoiceStatus.Pending, invoice.Status);
    }

    [Fact]
    public void Generating_Invoice_Sets_Pending_Status()
    {
        var draft = new InvoiceGenerationDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Money(100m, "USD"),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 7, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));

        var invoice = Invoice.Generate(draft);

        Assert.Equal(InvoiceStatus.Pending, invoice.Status);
    }

    [Fact]
    public void Generating_Invoice_Raises_InvoiceGenerated_Event()
    {
        var draft = new InvoiceGenerationDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Money(100m, "USD"),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 7, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));

        var invoice = Invoice.Generate(draft);

        var domainEvent = Assert.Single(invoice.DomainEvents);
        var generatedEvent = Assert.IsType<InvoiceGeneratedDomainEvent>(domainEvent);

        Assert.Equal(invoice.Id, generatedEvent.InvoiceId);
        Assert.Equal(invoice.SubscriptionId, generatedEvent.SubscriptionId);
        Assert.Equal(invoice.CustomerId, generatedEvent.CustomerId);
        Assert.Equal(invoice.Amount.Amount, generatedEvent.Amount);
        Assert.Equal(invoice.Amount.Currency, generatedEvent.Currency);
        Assert.Equal(draft.IssuedOnUtc, generatedEvent.OccurredOnUtc);
    }

    [Fact]
    public void PaidOnUtc_Is_Null_For_Pending_Invoice()
    {
        var draft = new InvoiceGenerationDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Money(100m, "USD"),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 7, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));

        var invoice = Invoice.Generate(draft);

        Assert.Null(invoice.PaidOnUtc);
        Assert.Null(invoice.PaymentMode);
    }

    [Theory]
    [InlineData(PaymentMode.Cash)]
    [InlineData(PaymentMode.Check)]
    [InlineData(PaymentMode.Online)]
    public void Marking_Invoice_As_Paid_With_Various_PaymentModes_Succeeds(PaymentMode paymentMode)
    {
        var draft = new InvoiceGenerationDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Money(100m, "USD"),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 7, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));

        var invoice = Invoice.Generate(draft);
        invoice.ClearDomainEvents();

        var paidOn = new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc);
        invoice.MarkAsPaid(paidOn, paymentMode);

        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(paidOn, invoice.PaidOnUtc);
        Assert.Equal(paymentMode, invoice.PaymentMode);
    }

    [Fact]
    public void Marking_Invoice_As_Paid_Raises_PaymentReceived_Event()
    {
        var draft = new InvoiceGenerationDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Money(100m, "USD"),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 7, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));

        var invoice = Invoice.Generate(draft);
        invoice.ClearDomainEvents();

        var paidOn = new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc);
        invoice.MarkAsPaid(paidOn, PaymentMode.Check);

        var domainEvent = Assert.Single(invoice.DomainEvents);
        var paymentEvent = Assert.IsType<PaymentReceivedDomainEvent>(domainEvent);

        Assert.Equal(invoice.Id, paymentEvent.InvoiceId);
        Assert.Equal(invoice.SubscriptionId, paymentEvent.SubscriptionId);
        Assert.Equal(invoice.CustomerId, paymentEvent.CustomerId);
        Assert.Equal(invoice.Amount.Amount, paymentEvent.Amount);
        Assert.Equal(invoice.Amount.Currency, paymentEvent.Currency);
        Assert.Equal(PaymentMode.Check, paymentEvent.PaymentMode);
        Assert.Equal(paidOn, paymentEvent.OccurredOnUtc);
    }

    [Fact]
    public void Marking_Already_Paid_Invoice_As_Paid_Again_Throws_DomainException()
    {
        var draft = new InvoiceGenerationDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Money(100m, "USD"),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 7, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));

        var invoice = Invoice.Generate(draft);

        var paidOn = new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc);
        invoice.MarkAsPaid(paidOn, PaymentMode.Check);

        var secondPaymentDate = new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc);
        var exception = Assert.Throws<DomainException>(() => invoice.MarkAsPaid(secondPaymentDate, PaymentMode.Online));

        Assert.Equal("Invoice cannot be paid twice.", exception.Message);
    }

    [Fact]
    public void Generated_Invoice_Has_All_Required_Properties()
    {
        var customerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var amount = new Money(150.50m, "EUR");
        var periodStart = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var periodEnd = new DateTime(2026, 4, 30, 10, 0, 0, DateTimeKind.Utc);
        var dueDate = new DateTime(2026, 5, 7, 10, 0, 0, DateTimeKind.Utc);
        var issuedOn = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);

        var draft = new InvoiceGenerationDraft(
            invoiceId,
            customerId,
            subscriptionId,
            amount,
            periodStart,
            periodEnd,
            dueDate,
            issuedOn);

        var invoice = Invoice.Generate(draft);

        Assert.Equal(invoiceId, invoice.Id);
        Assert.Equal(customerId, invoice.CustomerId);
        Assert.Equal(subscriptionId, invoice.SubscriptionId);
        Assert.Equal(amount, invoice.Amount);
        Assert.Equal(periodStart, invoice.PeriodStartUtc);
        Assert.Equal(periodEnd, invoice.PeriodEndUtc);
        Assert.Equal(dueDate, invoice.DueDateUtc);
        Assert.Equal(issuedOn, invoice.IssuedOnUtc);
        Assert.Equal(InvoiceStatus.Pending, invoice.Status);
    }
}
