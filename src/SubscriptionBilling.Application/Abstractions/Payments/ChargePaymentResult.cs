namespace SubscriptionBilling.Application.Abstractions.Payments;

public sealed record ChargePaymentResult(
    string PaymentReference,
    DateTime ProcessedOnUtc);
