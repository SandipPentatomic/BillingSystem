using SubscriptionBilling.Application.Exceptions;
using SubscriptionBilling.Application.Features.Invoices;
using SubscriptionBilling.Application.Tests.Support;
using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Billing;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Application.Tests.Features.Invoices;

public sealed class PayInvoiceCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Throws_When_Invoice_Does_Not_Exist()
    {
        var handler = new PayInvoiceCommandHandler(
            new FakeClock(DateTime.UtcNow),
            new FakeInvoiceRepository(),
            new SpyUnitOfWork());

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(
            new PayInvoiceCommand(Guid.NewGuid(), PaymentMode.Online, "invoice-key"),
            CancellationToken.None));

        Assert.Contains("was not found", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_Marks_Invoice_As_Paid_And_Persists_Changes()
    {
        var now = new DateTime(2026, 4, 24, 13, 0, 0, DateTimeKind.Utc);
        var invoiceRepository = new FakeInvoiceRepository();
        var unitOfWork = new SpyUnitOfWork();
        var invoice = Invoice.Generate(new InvoiceGenerationDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Money(59m, "USD"),
            now.AddDays(-30),
            now.AddDays(-1),
            now.AddDays(6),
            now.AddDays(-1)));
        invoiceRepository.Seed(invoice);

        var handler = new PayInvoiceCommandHandler(new FakeClock(now), invoiceRepository, unitOfWork);

        var result = await handler.HandleAsync(
            new PayInvoiceCommand(invoice.Id, PaymentMode.Check, "invoice-key"),
            CancellationToken.None);

        Assert.Equal(invoice.Id, result.InvoiceId);
        Assert.Equal("Paid", result.Status);
        Assert.Equal(now, result.PaidOnUtc);
        Assert.Equal(PaymentMode.Check, result.PaymentMode);
        Assert.Equal(PaymentMode.Check, invoice.PaymentMode);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }
}
