using SubscriptionBilling.Application.Abstractions.CQRS;

namespace SubscriptionBilling.Application.Features.Invoices;

public sealed record GetInvoicesQuery(
    Guid? CustomerId,
    Guid? SubscriptionId,
    string? Status) : IQuery<IReadOnlyCollection<InvoiceListItem>>;
