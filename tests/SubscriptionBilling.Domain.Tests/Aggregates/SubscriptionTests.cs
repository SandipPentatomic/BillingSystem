using SubscriptionBilling.Domain.Abstractions;
using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.Events;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Domain.Tests.Aggregates;

public sealed class SubscriptionTests
{
    [Fact]
    public void Creating_Subscription_With_Valid_Data_Succeeds()
    {
        var customerId = Guid.NewGuid();
        var planName = "Growth";
        var price = new Money(59m, "USD");
        var billingCycle = new BillingCycle(1, BillingIntervalUnit.Months);
        var activatedOn = DateTime.UtcNow;

        var subscription = Subscription.Create(customerId, planName, price, billingCycle, activatedOn);

        Assert.NotEqual(Guid.Empty, subscription.Id);
        Assert.Equal(customerId, subscription.CustomerId);
        Assert.Equal(planName, subscription.PlanName);
        Assert.Equal(price, subscription.Price);
        Assert.Equal(billingCycle, subscription.BillingCycle);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Equal(activatedOn, subscription.ActivatedOnUtc);
        Assert.Equal(activatedOn, subscription.CurrentPeriodStartUtc);
    }

    [Fact]
    public void Creating_Subscription_With_Empty_CustomerId_Throws_DomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            Subscription.Create(
                Guid.Empty,
                "Growth",
                new Money(59m, "USD"),
                new BillingCycle(1, BillingIntervalUnit.Months),
                DateTime.UtcNow));

        Assert.Equal("Customer id is required.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Creating_Subscription_With_Empty_PlanName_Throws_DomainException(string? emptyPlanName)
    {
        var exception = Assert.Throws<DomainException>(() =>
            Subscription.Create(
                Guid.NewGuid(),
                emptyPlanName!,
                new Money(59m, "USD"),
                new BillingCycle(1, BillingIntervalUnit.Months),
                DateTime.UtcNow));

        Assert.Equal("Plan name is required.", exception.Message);
    }

    [Fact]
    public void Creating_Subscription_Sets_NextBillingDate()
    {
        var customerId = Guid.NewGuid();
        var activatedOn = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        var billingCycle = new BillingCycle(1, BillingIntervalUnit.Months);

        var subscription = Subscription.Create(
            customerId,
            "Growth",
            new Money(59m, "USD"),
            billingCycle,
            activatedOn);

        var expectedNextBillingDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        Assert.Equal(expectedNextBillingDate, subscription.NextBillingDateUtc);
    }

    [Fact]
    public void Creating_Subscription_Trims_PlanName()
    {
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "  Growth  ",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            DateTime.UtcNow);

        Assert.Equal("Growth", subscription.PlanName);
    }

    [Fact]
    public void Creating_Subscription_Raises_SubscriptionActivated_Event()
    {
        var customerId = Guid.NewGuid();
        var activatedOn = new DateTime(2026, 4, 21, 10, 0, 0, DateTimeKind.Utc);

        var subscription = Subscription.Create(
            customerId,
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            activatedOn);

        var domainEvent = Assert.Single(subscription.DomainEvents);
        var activatedEvent = Assert.IsType<SubscriptionActivatedDomainEvent>(domainEvent);

        Assert.Equal(subscription.Id, activatedEvent.SubscriptionId);
        Assert.Equal(customerId, activatedEvent.CustomerId);
        Assert.Equal(activatedOn, activatedEvent.OccurredOnUtc);
    }

    [Fact]
    public void InitialInvoiceNotGenerated_On_Creation()
    {
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            DateTime.UtcNow);

        Assert.False(subscription.InitialInvoiceGenerated);
    }

    [Fact]
    public void CancelledOnUtc_Is_Null_On_Creation()
    {
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            DateTime.UtcNow);

        Assert.Null(subscription.CancelledOnUtc);
    }

    [Fact]
    public void Generating_Initial_Invoice_On_Active_Subscription_Succeeds()
    {
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            DateTime.UtcNow);

        subscription.ClearDomainEvents();

        var issuedOn = DateTime.UtcNow;
        var draft = subscription.GenerateInitialInvoice(issuedOn);

        Assert.NotEqual(Guid.Empty, draft.InvoiceId);
        Assert.Equal(subscription.CustomerId, draft.CustomerId);
        Assert.Equal(subscription.Id, draft.SubscriptionId);
        Assert.Equal(subscription.Price, draft.Amount);
        Assert.Equal(subscription.CurrentPeriodStartUtc, draft.PeriodStartUtc);
        Assert.Equal(subscription.NextBillingDateUtc, draft.PeriodEndUtc);
        Assert.Equal(issuedOn, draft.IssuedOnUtc);
        Assert.Equal(issuedOn.AddDays(7), draft.DueDateUtc);
        Assert.True(subscription.InitialInvoiceGenerated);
    }

    [Fact]
    public void Generating_Initial_Invoice_On_Cancelled_Subscription_Throws_DomainException()
    {
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            DateTime.UtcNow);

        subscription.Cancel(DateTime.UtcNow);

        var exception = Assert.Throws<DomainException>(() => subscription.GenerateInitialInvoice(DateTime.UtcNow));

        Assert.Equal("Only active subscriptions can generate invoices.", exception.Message);
    }

    [Fact]
    public void Generating_Initial_Invoice_Twice_Throws_DomainException()
    {
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            DateTime.UtcNow);

        subscription.GenerateInitialInvoice(DateTime.UtcNow);

        var exception = Assert.Throws<DomainException>(() => subscription.GenerateInitialInvoice(DateTime.UtcNow));

        Assert.Equal("Initial invoice has already been generated.", exception.Message);
    }

    [Fact]
    public void Generating_Due_Invoices_On_Active_Subscription()
    {
        var activatedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            activatedOn);

        subscription.ClearDomainEvents();

        // Generate initial invoice
        subscription.GenerateInitialInvoice(activatedOn.AddMonths(1));

        // Now generate due invoices for 3 months total
        var issuedOn = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        var drafts = subscription.GenerateDueInvoices(issuedOn);

        Assert.NotEmpty(drafts);
    }

    [Fact]
    public void Generating_Due_Invoices_On_Cancelled_Subscription_Returns_Empty()
    {
        var activatedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            activatedOn);

        subscription.Cancel(activatedOn.AddMonths(2));

        var drafts = subscription.GenerateDueInvoices(activatedOn.AddMonths(3));

        Assert.Empty(drafts);
    }

    [Fact]
    public void Generating_Due_Invoices_Updates_BillingDates()
    {
        var activatedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            activatedOn);

        var originalNextBillingDate = subscription.NextBillingDateUtc;

        var issuedOn = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        subscription.GenerateDueInvoices(issuedOn);

        Assert.NotEqual(originalNextBillingDate, subscription.NextBillingDateUtc);
    }

    [Fact]
    public void Cancelling_Active_Subscription_Succeeds()
    {
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            DateTime.UtcNow);

        var cancelledOn = DateTime.UtcNow.AddDays(1);
        subscription.Cancel(cancelledOn);

        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
        Assert.Equal(cancelledOn, subscription.CancelledOnUtc);
    }

    [Fact]
    public void Cancelling_Already_Cancelled_Subscription_Is_Idempotent()
    {
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            DateTime.UtcNow);

        var cancelledOn = DateTime.UtcNow.AddDays(1);
        subscription.Cancel(cancelledOn);

        var secondCancelDate = cancelledOn.AddDays(1);
        subscription.Cancel(secondCancelDate);

        // Should still have the first cancellation date
        Assert.Equal(cancelledOn, subscription.CancelledOnUtc);
        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
    }

    [Fact]
    public void Creating_Subscription_Generates_Unique_Ids()
    {
        var sub1 = Subscription.Create(
            Guid.NewGuid(),
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            DateTime.UtcNow);

        var sub2 = Subscription.Create(
            Guid.NewGuid(),
            "Growth",
            new Money(59m, "USD"),
            new BillingCycle(1, BillingIntervalUnit.Months),
            DateTime.UtcNow);

        Assert.NotEqual(sub1.Id, sub2.Id);
    }
}
