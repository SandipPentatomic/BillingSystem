using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Application.ReadModels;

namespace SubscriptionBilling.Application.Features.Invoices;

public sealed class GetInvoicesQueryHandler : IQueryHandler<GetInvoicesQuery, PagedResult<InvoiceListItem>>
{
    private readonly IInvoiceReadRepository _invoiceReadRepository;

    public GetInvoicesQueryHandler(IInvoiceReadRepository invoiceReadRepository)
    {
        _invoiceReadRepository = invoiceReadRepository;
    }

    public Task<PagedResult<InvoiceListItem>> HandleAsync(GetInvoicesQuery query, CancellationToken cancellationToken)
    {
        return _invoiceReadRepository.ListAsync(
            query.CustomerId,
            query.SubscriptionId,
            query.Status,
            query.PageNumber,
            query.PageSize,
            cancellationToken);
    }
}
