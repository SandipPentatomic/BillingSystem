using SubscriptionBilling.Domain.Abstractions;
using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Domain.Events;

public sealed record PaymentReceivedDomainEvent(
    Guid InvoiceId,
    Guid SubscriptionId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    PaymentMode PaymentMode,
    DateTime OccurredOnUtc) : IDomainEvent;
