using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.Events;
using SubscriptionBilling.Domain.ValueObjects;
using SubscriptionBilling.Infrastructure.Persistence;
using SubscriptionBilling.Infrastructure.Tests.Support;

namespace SubscriptionBilling.Infrastructure.Tests.Persistence;

public sealed class EfUnitOfWorkTests
{
    [Fact]
    public async Task SaveChangesAsync_Persists_Domain_Events_To_Outbox_And_Clears_Them()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var unitOfWork = new EfUnitOfWork(dbContext);
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            new DateTime(2026, 4, 24, 8, 0, 0, DateTimeKind.Utc));

        dbContext.Subscriptions.Add(subscription);

        var affectedRows = await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var outboxMessage = Assert.Single(dbContext.OutboxMessages);
        Assert.True(affectedRows > 0);
        Assert.Empty(subscription.DomainEvents);
        Assert.Contains(nameof(SubscriptionActivatedDomainEvent), outboxMessage.Type);
        Assert.Contains(subscription.Id.ToString(), outboxMessage.Content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveChangesAsync_Works_When_No_Domain_Events_Are_Present()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var unitOfWork = new EfUnitOfWork(dbContext);

        var affectedRows = await unitOfWork.SaveChangesAsync(CancellationToken.None);

        Assert.Equal(0, affectedRows);
        Assert.Empty(dbContext.OutboxMessages);
    }
}
