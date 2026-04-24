namespace SubscriptionBilling.Application.Features.Billing;

public sealed record RunBillingCycleResult(int SubscriptionsProcessed, int InvoicesGenerated, DateTime ExecutedOnUtc);
