using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Application.Abstractions.Payments;

public sealed record ChargePaymentRequest(
    Guid InvoiceId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    PaymentMode PaymentMode);
