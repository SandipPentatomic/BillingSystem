namespace SubscriptionBilling.Application.Abstractions.CQRS;

public interface IIdempotentRequest
{
    string IdempotencyKey { get; }
}
