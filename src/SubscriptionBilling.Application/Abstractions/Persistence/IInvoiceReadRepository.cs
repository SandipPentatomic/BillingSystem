using SubscriptionBilling.Application.Features.Invoices;

namespace SubscriptionBilling.Application.Abstractions.Persistence;

public interface IInvoiceReadRepository
{
    Task<IReadOnlyCollection<InvoiceListItem>> ListAsync(
        Guid? customerId,
        Guid? subscriptionId,
        string? status,
        CancellationToken cancellationToken);
}
