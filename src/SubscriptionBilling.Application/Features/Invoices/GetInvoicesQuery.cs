using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.ReadModels;
using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Application.Features.Invoices;

public sealed record GetInvoicesQuery(
    Guid? CustomerId,
    Guid? SubscriptionId,
    InvoiceStatus? Status,
    int PageNumber,
    int PageSize) : IQuery<PagedResult<InvoiceListItem>>;
