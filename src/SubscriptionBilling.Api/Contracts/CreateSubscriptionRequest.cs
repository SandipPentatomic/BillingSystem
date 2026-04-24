using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Api.Contracts;

public sealed record CreateSubscriptionRequest(
    Guid CustomerId,
    string PlanName,
    decimal Amount,
    string Currency,
    int BillingInterval = 1,
    BillingIntervalUnit BillingIntervalUnit = BillingIntervalUnit.Months);
