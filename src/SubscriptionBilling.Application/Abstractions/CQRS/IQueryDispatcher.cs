namespace SubscriptionBilling.Application.Abstractions.CQRS;

public interface IQueryDispatcher
{
    Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
