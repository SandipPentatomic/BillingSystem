using SubscriptionBilling.Domain.Abstractions;

namespace SubscriptionBilling.Domain.Events;

public sealed record InvoiceGeneratedDomainEvent(
    Guid InvoiceId,
    Guid SubscriptionId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    DateTime OccurredOnUtc) : IDomainEvent;
