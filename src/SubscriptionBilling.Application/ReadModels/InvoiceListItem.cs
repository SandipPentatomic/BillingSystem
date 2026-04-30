using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Application.ReadModels;

public sealed record InvoiceListItem(
    Guid InvoiceId,
    Guid CustomerId,
    Guid SubscriptionId,
    decimal Amount,
    string Currency,
    string Status,
    DateTime PeriodStartUtc,
    DateTime PeriodEndUtc,
    DateTime DueDateUtc,
    DateTime IssuedOnUtc,
    DateTime? PaidOnUtc,
    PaymentMode? PaymentMode,
    string? PaymentReference);
