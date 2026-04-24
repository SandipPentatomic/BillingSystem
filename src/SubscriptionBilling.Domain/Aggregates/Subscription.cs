using System.Diagnostics.CodeAnalysis;
using SubscriptionBilling.Domain.Abstractions;
using SubscriptionBilling.Domain.Billing;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.Events;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Domain.Aggregates;

public sealed class Subscription : AggregateRoot
{
    [ExcludeFromCodeCoverage]
    private Subscription()
    {
    }

    private Subscription(
        Guid id,
        Guid customerId,
        string planName,
        Money price,
        BillingCycle billingCycle,
        SubscriptionStatus status,
        DateTime currentPeriodStartUtc,
        DateTime nextBillingDateUtc,
        DateTime activatedOnUtc)
    {
        Id = id;
        CustomerId = customerId;
        PlanName = planName;
        Price = price;
        BillingCycle = billingCycle;
        Status = status;
        CurrentPeriodStartUtc = currentPeriodStartUtc;
        NextBillingDateUtc = nextBillingDateUtc;
        ActivatedOnUtc = activatedOnUtc;
    }

    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public string PlanName { get; private set; } = string.Empty;

    public Money Price { get; private set; } = null!;

    public BillingCycle BillingCycle { get; private set; } = null!;

    public SubscriptionStatus Status { get; private set; }

    public DateTime CurrentPeriodStartUtc { get; private set; }

    public DateTime NextBillingDateUtc { get; private set; }

    public bool InitialInvoiceGenerated { get; private set; }

    public DateTime ActivatedOnUtc { get; private set; }

    public DateTime? CancelledOnUtc { get; private set; }

    public static Subscription Create(
        Guid customerId,
        string planName,
        Money price,
        BillingCycle billingCycle,
        DateTime activatedOnUtc)
    {
        if (customerId == Guid.Empty)
        {
            throw new DomainException("Customer id is required.");
        }

        if (string.IsNullOrWhiteSpace(planName))
        {
            throw new DomainException("Plan name is required.");
        }

        var periodStartUtc = activatedOnUtc;
        var nextBillingDateUtc = billingCycle.AddTo(periodStartUtc);

        var subscription = new Subscription(
            Guid.NewGuid(),
            customerId,
            planName.Trim(),
            price,
            billingCycle,
            SubscriptionStatus.Active,
            periodStartUtc,
            nextBillingDateUtc,
            activatedOnUtc);

        subscription.Raise(new SubscriptionActivatedDomainEvent(subscription.Id, customerId, activatedOnUtc));

        return subscription;
    }

    public InvoiceGenerationDraft GenerateInitialInvoice(DateTime issuedOnUtc)
    {
        EnsureActive();

        if (InitialInvoiceGenerated)
        {
            throw new DomainException("Initial invoice has already been generated.");
        }

        InitialInvoiceGenerated = true;

        return BuildDraft(CurrentPeriodStartUtc, NextBillingDateUtc, issuedOnUtc);
    }

    public IReadOnlyCollection<InvoiceGenerationDraft> GenerateDueInvoices(DateTime issuedOnUtc)
    {
        if (Status == SubscriptionStatus.Cancelled)
        {
            return [];
        }

        var drafts = new List<InvoiceGenerationDraft>();
        while (NextBillingDateUtc <= issuedOnUtc)
        {
            var periodStartUtc = NextBillingDateUtc;
            var upcomingBillingDateUtc = BillingCycle.AddTo(periodStartUtc);

            drafts.Add(BuildDraft(periodStartUtc, upcomingBillingDateUtc, issuedOnUtc));

            CurrentPeriodStartUtc = periodStartUtc;
            NextBillingDateUtc = upcomingBillingDateUtc;
        }

        return drafts;
    }

    public void Cancel(DateTime cancelledOnUtc)
    {
        if (Status == SubscriptionStatus.Cancelled)
        {
            return;
        }

        Status = SubscriptionStatus.Cancelled;
        CancelledOnUtc = cancelledOnUtc;
    }

    private InvoiceGenerationDraft BuildDraft(DateTime periodStartUtc, DateTime nextBillingDateUtc, DateTime issuedOnUtc)
    {
        return new InvoiceGenerationDraft(
            Guid.NewGuid(),
            CustomerId,
            Id,
            Price,
            periodStartUtc,
            nextBillingDateUtc,
            issuedOnUtc.AddDays(7),
            issuedOnUtc);
    }

    private void EnsureActive()
    {
        if (Status != SubscriptionStatus.Active)
        {
            throw new DomainException("Only active subscriptions can generate invoices.");
        }
    }
}
