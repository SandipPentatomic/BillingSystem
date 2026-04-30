using SubscriptionBilling.Application.ReadModels;
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
        newerInvoice.MarkAsPaid(new DateTime(2026, 4, 24, 10, 0, 0, DateTimeKind.Utc), PaymentMode.Cash, "CASH-REF-001");

        dbContext.Invoices.AddRange(olderInvoice, newerInvoice, CreateInvoice(Guid.NewGuid(), Guid.NewGuid(), new DateTime(2026, 4, 22, 9, 0, 0, DateTimeKind.Utc)));
        await dbContext.SaveChangesAsync();

        var invoices = await repository.ListAsync(customerId, subscriptionId, InvoiceStatus.Paid, 1, 10, CancellationToken.None);

        var invoice = Assert.Single(invoices.Items);
        Assert.Equal(newerInvoice.Id, invoice.InvoiceId);
        Assert.Equal(PaymentMode.Cash, invoice.PaymentMode);
        Assert.Equal("CASH-REF-001", invoice.PaymentReference);
        Assert.Equal("Paid", invoice.Status);
        Assert.Equal(1, invoices.PageNumber);
        Assert.Equal(10, invoices.PageSize);
        Assert.Equal(1, invoices.TotalCount);
    }

    [Fact]
    public async Task ListAsync_Applies_Pagination()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var repository = new InvoiceReadRepository(dbContext);
        var customerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();

        for (var index = 0; index < 3; index++)
        {
            dbContext.Invoices.Add(CreateInvoice(customerId, subscriptionId, new DateTime(2026, 4, 20 + index, 9, 0, 0, DateTimeKind.Utc)));
        }

        await dbContext.SaveChangesAsync();

        var invoices = await repository.ListAsync(customerId, subscriptionId, null, 2, 1, CancellationToken.None);

        var invoice = Assert.Single(invoices.Items);
        Assert.Equal(2, invoices.PageNumber);
        Assert.Equal(1, invoices.PageSize);
        Assert.Equal(3, invoices.TotalCount);
        Assert.Equal(new DateTime(2026, 4, 21, 9, 0, 0, DateTimeKind.Utc), invoice.IssuedOnUtc);
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
