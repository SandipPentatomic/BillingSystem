using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Application.Features.Subscriptions;

public sealed record CreateSubscriptionResult(
    Guid SubscriptionId,
    Guid CustomerId,
    string PlanName,
    string Status,
    DateTime CurrentPeriodStartUtc,
    DateTime NextBillingDateUtc,
    Guid InitialInvoiceId,
    decimal Amount,
    string Currency,
    int BillingInterval,
    BillingIntervalUnit BillingIntervalUnit);
