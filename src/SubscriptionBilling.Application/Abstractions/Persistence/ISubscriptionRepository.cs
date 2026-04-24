using SubscriptionBilling.Domain.Aggregates;

namespace SubscriptionBilling.Application.Abstractions.Persistence;

public interface ISubscriptionRepository
{
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken);

    Task<Subscription?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Subscription>> ListDueForBillingAsync(DateTime asOfUtc, CancellationToken cancellationToken);
}
