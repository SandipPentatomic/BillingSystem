using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Abstractions;
using SubscriptionBilling.Domain.Billing;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Domain.Tests;

public sealed class InvoiceTests
{
    [Fact]
    public void Paying_Invoice_Marks_It_As_Paid()
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

        invoice.MarkAsPaid(new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc), PaymentMode.Online);

        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.NotNull(invoice.PaidOnUtc);
        Assert.Equal(PaymentMode.Online, invoice.PaymentMode);
    }

    [Fact]
    public void Paying_Invoice_Twice_Throws()
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

        invoice.MarkAsPaid(new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc), PaymentMode.Cash);

        var exception = Assert.Throws<DomainException>(() =>
            invoice.MarkAsPaid(new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc), PaymentMode.Check));

        Assert.Equal("Invoice cannot be paid twice.", exception.Message);
    }
}
