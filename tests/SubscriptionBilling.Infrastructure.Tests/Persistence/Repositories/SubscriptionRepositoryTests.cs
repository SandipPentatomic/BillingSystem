using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.ValueObjects;
using SubscriptionBilling.Infrastructure.Persistence.Repositories;
using SubscriptionBilling.Infrastructure.Tests.Support;

namespace SubscriptionBilling.Infrastructure.Tests.Persistence.Repositories;

public sealed class SubscriptionRepositoryTests
{
    [Fact]
    public async Task AddAsync_And_GetByIdAsync_Persist_Subscription()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var repository = new SubscriptionRepository(dbContext);
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            DateTime.UtcNow);

        await repository.AddAsync(subscription, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var storedSubscription = await repository.GetByIdAsync(subscription.Id, CancellationToken.None);

        Assert.NotNull(storedSubscription);
        Assert.Equal(subscription.Id, storedSubscription.Id);
    }

    [Fact]
    public async Task ListDueForBillingAsync_Returns_Only_Active_Subscriptions_That_Are_Due()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var repository = new SubscriptionRepository(dbContext);
        var asOfUtc = new DateTime(2026, 4, 24, 10, 0, 0, DateTimeKind.Utc);

        var dueSubscription = Subscription.Create(
            Guid.NewGuid(),
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Days),
            asOfUtc.AddDays(-2));

        var futureSubscription = Subscription.Create(
            Guid.NewGuid(),
            "Starter",
            new Money(19m, "USD"),
            new BillingCycle(10, BillingIntervalUnit.Days),
            asOfUtc);

        var cancelledSubscription = Subscription.Create(
            Guid.NewGuid(),
            "Pro",
            new Money(99m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Days),
            asOfUtc.AddDays(-2));
        cancelledSubscription.Cancel(asOfUtc.AddDays(-1));

        dbContext.Subscriptions.AddRange(dueSubscription, futureSubscription, cancelledSubscription);
        await dbContext.SaveChangesAsync();

        var dueSubscriptions = await repository.ListDueForBillingAsync(asOfUtc, CancellationToken.None);

        var subscription = Assert.Single(dueSubscriptions);
        Assert.Equal(dueSubscription.Id, subscription.Id);
    }
}
