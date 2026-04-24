using SubscriptionBilling.Application.Features.Billing;
using SubscriptionBilling.Application.Tests.Support;
using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Application.Tests.Features.Billing;

public sealed class RunBillingCycleCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Generates_Invoices_For_Due_Subscriptions_And_Skips_Empty_Batches()
    {
        var now = new DateTime(2026, 4, 24, 14, 0, 0, DateTimeKind.Utc);
        var subscriptionRepository = new FakeSubscriptionRepository();
        var invoiceRepository = new FakeInvoiceRepository();
        var unitOfWork = new SpyUnitOfWork();

        var dueSubscription = Subscription.Create(
            Guid.NewGuid(),
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Days),
            now.AddDays(-3));
        dueSubscription.GenerateInitialInvoice(now.AddDays(-3));

        var cancelledSubscription = Subscription.Create(
            Guid.NewGuid(),
            "Starter",
            new Money(19m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Days),
            now.AddDays(-3));
        cancelledSubscription.GenerateInitialInvoice(now.AddDays(-3));
        cancelledSubscription.Cancel(now.AddDays(-2));

        subscriptionRepository.SetDueSubscriptions(dueSubscription, cancelledSubscription);

        var handler = new RunBillingCycleCommandHandler(
            new FakeClock(now),
            subscriptionRepository,
            invoiceRepository,
            unitOfWork);

        var result = await handler.HandleAsync(new RunBillingCycleCommand(), CancellationToken.None);

        var addedRange = Assert.Single(invoiceRepository.AddedRanges);

        Assert.Equal(2, result.SubscriptionsProcessed);
        Assert.Equal(3, result.InvoicesGenerated);
        Assert.Equal(now, result.ExecutedOnUtc);
        Assert.Equal(3, addedRange.Count);
        Assert.All(addedRange, invoice => Assert.Equal("Pending", invoice.Status.ToString()));
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_Returns_Zero_When_No_Subscriptions_Are_Due()
    {
        var now = new DateTime(2026, 4, 24, 14, 30, 0, DateTimeKind.Utc);
        var handler = new RunBillingCycleCommandHandler(
            new FakeClock(now),
            new FakeSubscriptionRepository(),
            new FakeInvoiceRepository(),
            new SpyUnitOfWork());

        var result = await handler.HandleAsync(new RunBillingCycleCommand(), CancellationToken.None);

        Assert.Equal(0, result.SubscriptionsProcessed);
        Assert.Equal(0, result.InvoicesGenerated);
        Assert.Equal(now, result.ExecutedOnUtc);
    }
}
