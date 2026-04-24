using SubscriptionBilling.Application.Exceptions;
using SubscriptionBilling.Application.Features.Subscriptions;
using SubscriptionBilling.Application.Tests.Support;
using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Application.Tests.Features.Subscriptions;

public sealed class CancelSubscriptionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Throws_When_Subscription_Does_Not_Exist()
    {
        var handler = new CancelSubscriptionCommandHandler(
            new FakeClock(DateTime.UtcNow),
            new FakeSubscriptionRepository(),
            new SpyUnitOfWork());

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(
            new CancelSubscriptionCommand(Guid.NewGuid(), "cancel-key"),
            CancellationToken.None));

        Assert.Contains("was not found", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_Cancels_Subscription_And_Persists_Changes()
    {
        var now = new DateTime(2026, 4, 24, 12, 0, 0, DateTimeKind.Utc);
        var subscriptionRepository = new FakeSubscriptionRepository();
        var unitOfWork = new SpyUnitOfWork();
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            now.AddDays(-2));
        subscriptionRepository.Seed(subscription);

        var handler = new CancelSubscriptionCommandHandler(new FakeClock(now), subscriptionRepository, unitOfWork);

        var result = await handler.HandleAsync(
            new CancelSubscriptionCommand(subscription.Id, "cancel-key"),
            CancellationToken.None);

        Assert.Equal(subscription.Id, result.SubscriptionId);
        Assert.Equal("Cancelled", result.Status);
        Assert.Equal(now, result.CancelledOnUtc);
        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }
}
