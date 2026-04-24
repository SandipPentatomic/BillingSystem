using Microsoft.EntityFrameworkCore;
using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Domain.Aggregates;

namespace SubscriptionBilling.Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly BillingDbContext _dbContext;

    public InvoiceRepository(BillingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        return _dbContext.Invoices.AddAsync(invoice, cancellationToken).AsTask();
    }

    public Task AddRangeAsync(IEnumerable<Invoice> invoices, CancellationToken cancellationToken)
    {
        return _dbContext.Invoices.AddRangeAsync(invoices, cancellationToken);
    }

    public Task<Invoice?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        return _dbContext.Invoices.SingleOrDefaultAsync(invoice => invoice.Id == invoiceId, cancellationToken);
    }
}
