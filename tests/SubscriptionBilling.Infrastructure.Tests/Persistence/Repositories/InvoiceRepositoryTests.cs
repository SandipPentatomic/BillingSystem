using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Billing;
using SubscriptionBilling.Domain.ValueObjects;
using SubscriptionBilling.Infrastructure.Persistence.Repositories;
using SubscriptionBilling.Infrastructure.Tests.Support;

namespace SubscriptionBilling.Infrastructure.Tests.Persistence.Repositories;

public sealed class InvoiceRepositoryTests
{
    [Fact]
    public async Task AddAsync_And_GetByIdAsync_Persist_Invoice()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var repository = new InvoiceRepository(dbContext);
        var invoice = CreateInvoice(DateTime.UtcNow);

        await repository.AddAsync(invoice, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var storedInvoice = await repository.GetByIdAsync(invoice.Id, CancellationToken.None);

        Assert.NotNull(storedInvoice);
        Assert.Equal(invoice.Id, storedInvoice.Id);
    }

    [Fact]
    public async Task AddRangeAsync_Persists_Multiple_Invoices()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var repository = new InvoiceRepository(dbContext);
        var invoices = new[]
        {
            CreateInvoice(DateTime.UtcNow.AddDays(-1)),
            CreateInvoice(DateTime.UtcNow)
        };

        await repository.AddRangeAsync(invoices, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        Assert.Equal(2, dbContext.Invoices.Count());
    }

    private static Invoice CreateInvoice(DateTime issuedOnUtc)
    {
        return Invoice.Generate(new InvoiceGenerationDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Money(59m, "USD"),
            issuedOnUtc.AddDays(-30),
            issuedOnUtc,
            issuedOnUtc.AddDays(7),
            issuedOnUtc));
    }
}
