using SubscriptionBilling.Application.Exceptions;
using SubscriptionBilling.Application.Features.Subscriptions;
using SubscriptionBilling.Application.Tests.Support;
using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Application.Tests.Features.Subscriptions;

public sealed class CreateSubscriptionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Throws_When_Customer_Does_Not_Exist()
    {
        var handler = new CreateSubscriptionCommandHandler(
            new FakeClock(DateTime.UtcNow),
            new FakeCustomerRepository(),
            new FakeSubscriptionRepository(),
            new FakeInvoiceRepository(),
            new SpyUnitOfWork());

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(
            new CreateSubscriptionCommand(Guid.NewGuid(), "Growth", 59m, "USD", 1, BillingIntervalUnit.Months, "subscription-key"),
            CancellationToken.None));

        Assert.Contains("was not found", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_Creates_Subscription_Initial_Invoice_And_Persists_Changes()
    {
        var now = new DateTime(2026, 4, 24, 10, 0, 0, DateTimeKind.Utc);
        var clock = new FakeClock(now);
        var customerRepository = new FakeCustomerRepository();
        var subscriptionRepository = new FakeSubscriptionRepository();
        var invoiceRepository = new FakeInvoiceRepository();
        var unitOfWork = new SpyUnitOfWork();
        var customer = Customer.Create("Alice", "alice@example.com", now.AddMinutes(-5));
        customerRepository.Seed(customer);

        var handler = new CreateSubscriptionCommandHandler(
            clock,
            customerRepository,
            subscriptionRepository,
            invoiceRepository,
            unitOfWork);

        var result = await handler.HandleAsync(
            new CreateSubscriptionCommand(customer.Id, "Growth", 59m, "USD", 15, BillingIntervalUnit.Minutes, "subscription-key"),
            CancellationToken.None);

        var subscription = Assert.Single(subscriptionRepository.AddedSubscriptions);
        var invoice = Assert.Single(invoiceRepository.AddedInvoices);

        Assert.Equal(subscription.Id, result.SubscriptionId);
        Assert.Equal(customer.Id, result.CustomerId);
        Assert.Equal("Growth", result.PlanName);
        Assert.Equal("Active", result.Status);
        Assert.Equal(now, result.CurrentPeriodStartUtc);
        Assert.Equal(now.AddMinutes(15), result.NextBillingDateUtc);
        Assert.Equal(invoice.Id, result.InitialInvoiceId);
        Assert.Equal(59m, result.Amount);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(15, result.BillingInterval);
        Assert.Equal(BillingIntervalUnit.Minutes, result.BillingIntervalUnit);
        Assert.True(subscription.InitialInvoiceGenerated);
        Assert.Equal(subscription.Id, invoice.SubscriptionId);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }
}
