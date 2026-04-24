using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Application.Features.Invoices;

public sealed record PayInvoiceResult(Guid InvoiceId, string Status, DateTime PaidOnUtc, PaymentMode PaymentMode);
