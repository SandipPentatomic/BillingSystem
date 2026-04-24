using SubscriptionBilling.Application.Abstractions.CQRS;

namespace SubscriptionBilling.Application.Features.Subscriptions;

public sealed record CancelSubscriptionCommand(
    Guid SubscriptionId,
    string IdempotencyKey) : ICommand<CancelSubscriptionResult>, IIdempotentRequest;
