using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Domain.Abstractions;

namespace SubscriptionBilling.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly BillingDbContext _dbContext;

    public EfUnitOfWork(BillingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregates = _dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToArray();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                _dbContext.OutboxMessages.Add(OutboxMessage.FromDomainEvent(domainEvent));
            }

            aggregate.ClearDomainEvents();
        }

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
