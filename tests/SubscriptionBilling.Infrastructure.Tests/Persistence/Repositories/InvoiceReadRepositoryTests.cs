using SubscriptionBilling.Application.Features.Invoices;
using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Billing;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.ValueObjects;
using SubscriptionBilling.Infrastructure.Persistence.Repositories;
using SubscriptionBilling.Infrastructure.Tests.Support;

namespace SubscriptionBilling.Infrastructure.Tests.Persistence.Repositories;

public sealed class InvoiceReadRepositoryTests
{
    [Fact]
    public async Task ListAsync_Filters_Orders_And_Projects_Invoices()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var repository = new InvoiceReadRepository(dbContext);
        var customerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var olderInvoice = CreateInvoice(customerId, subscriptionId, new DateTime(2026, 4, 20, 9, 0, 0, DateTimeKind.Utc));
        var newerInvoice = CreateInvoice(customerId, subscriptionId, new DateTime(2026, 4, 24, 9, 0, 0, DateTimeKind.Utc));
        newerInvoice.MarkAsPaid(new DateTime(2026, 4, 24, 10, 0, 0, DateTimeKind.Utc), PaymentMode.Cash);

        dbContext.Invoices.AddRange(olderInvoice, newerInvoice, CreateInvoice(Guid.NewGuid(), Guid.NewGuid(), new DateTime(2026, 4, 22, 9, 0, 0, DateTimeKind.Utc)));
        await dbContext.SaveChangesAsync();

        var invoices = await repository.ListAsync(customerId, subscriptionId, "Paid", CancellationToken.None);

        var invoice = Assert.Single(invoices);
        Assert.Equal(newerInvoice.Id, invoice.InvoiceId);
        Assert.Equal(PaymentMode.Cash, invoice.PaymentMode);
        Assert.Equal("Paid", invoice.Status);
    }

    [Fact]
    public async Task ListAsync_Returns_Empty_When_Status_Filter_Is_Invalid()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var repository = new InvoiceReadRepository(dbContext);

        var invoices = await repository.ListAsync(null, null, "not-a-status", CancellationToken.None);

        Assert.Empty(invoices);
    }

    private static Invoice CreateInvoice(Guid customerId, Guid subscriptionId, DateTime issuedOnUtc)
    {
        return Invoice.Generate(new InvoiceGenerationDraft(
            Guid.NewGuid(),
            customerId,
            subscriptionId,
            new Money(59m, "USD"),
            issuedOnUtc.AddDays(-30),
            issuedOnUtc,
            issuedOnUtc.AddDays(7),
            issuedOnUtc));
    }
}
