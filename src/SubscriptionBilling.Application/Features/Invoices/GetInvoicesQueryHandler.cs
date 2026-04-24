using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Abstractions.Persistence;

namespace SubscriptionBilling.Application.Features.Invoices;

public sealed class GetInvoicesQueryHandler : IQueryHandler<GetInvoicesQuery, IReadOnlyCollection<InvoiceListItem>>
{
    private readonly IInvoiceReadRepository _invoiceReadRepository;

    public GetInvoicesQueryHandler(IInvoiceReadRepository invoiceReadRepository)
    {
        _invoiceReadRepository = invoiceReadRepository;
    }

    public Task<IReadOnlyCollection<InvoiceListItem>> HandleAsync(GetInvoicesQuery query, CancellationToken cancellationToken)
    {
        return _invoiceReadRepository.ListAsync(query.CustomerId, query.SubscriptionId, query.Status, cancellationToken);
    }
}
