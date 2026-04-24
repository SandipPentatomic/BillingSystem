using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Domain.Billing;

public sealed record InvoiceGenerationDraft(
    Guid InvoiceId,
    Guid CustomerId,
    Guid SubscriptionId,
    Money Amount,
    DateTime PeriodStartUtc,
    DateTime PeriodEndUtc,
    DateTime DueDateUtc,
    DateTime IssuedOnUtc);
