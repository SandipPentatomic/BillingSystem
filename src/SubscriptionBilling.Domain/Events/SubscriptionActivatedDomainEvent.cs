using SubscriptionBilling.Domain.Abstractions;

namespace SubscriptionBilling.Domain.Events;

public sealed record SubscriptionActivatedDomainEvent(
    Guid SubscriptionId,
    Guid CustomerId,
    DateTime OccurredOnUtc) : IDomainEvent;
