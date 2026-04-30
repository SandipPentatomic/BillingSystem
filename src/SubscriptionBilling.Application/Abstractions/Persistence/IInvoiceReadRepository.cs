using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Application.ReadModels;

namespace SubscriptionBilling.Application.Abstractions.Persistence;

public interface IInvoiceReadRepository
{
    Task<PagedResult<InvoiceListItem>> ListAsync(
        Guid? customerId,
        Guid? subscriptionId,
        InvoiceStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}
