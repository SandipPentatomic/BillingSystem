namespace SubscriptionBilling.Application.Abstractions.Persistence;

public interface IIdempotencyStore
{
    Task<IAsyncDisposable> AcquireAsync(string idempotencyKey, CancellationToken cancellationToken);

    Task<string?> GetResponseAsync(string idempotencyKey, CancellationToken cancellationToken);

    Task SaveResponseAsync(string idempotencyKey, string responseJson, CancellationToken cancellationToken);
}
