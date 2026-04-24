using Microsoft.EntityFrameworkCore;
using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Infrastructure.Persistence.Repositories;

public sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly BillingDbContext _dbContext;

    public SubscriptionRepository(BillingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        return _dbContext.Subscriptions.AddAsync(subscription, cancellationToken).AsTask();
    }

    public Task<Subscription?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken)
    {
        return _dbContext.Subscriptions.SingleOrDefaultAsync(subscription => subscription.Id == subscriptionId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Subscription>> ListDueForBillingAsync(DateTime asOfUtc, CancellationToken cancellationToken)
    {
        return await _dbContext.Subscriptions
            .Where(subscription => subscription.Status == SubscriptionStatus.Active && subscription.NextBillingDateUtc <= asOfUtc)
            .ToListAsync(cancellationToken);
    }
}
