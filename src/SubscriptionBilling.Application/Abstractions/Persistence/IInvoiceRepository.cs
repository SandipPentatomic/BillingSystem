using SubscriptionBilling.Domain.Aggregates;

namespace SubscriptionBilling.Application.Abstractions.Persistence;

public interface IInvoiceRepository
{
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken);

    Task AddRangeAsync(IEnumerable<Invoice> invoices, CancellationToken cancellationToken);

    Task<Invoice?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken);
}
