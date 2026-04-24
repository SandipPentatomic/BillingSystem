namespace SubscriptionBilling.Application.Features.Subscriptions;

public sealed record CancelSubscriptionResult(Guid SubscriptionId, string Status, DateTime? CancelledOnUtc);
