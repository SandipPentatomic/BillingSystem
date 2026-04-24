namespace SubscriptionBilling.Application.Abstractions.Persistence;

public interface IIdempotencyStore
{
    Task<string?> GetResponseAsync(string idempotencyKey, CancellationToken cancellationToken);

    Task SaveResponseAsync(string idempotencyKey, string responseJson, CancellationToken cancellationToken);
}
