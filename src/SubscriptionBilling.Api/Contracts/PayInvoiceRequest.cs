using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Api.Contracts;

public sealed record PayInvoiceRequest(PaymentMode PaymentMode);
