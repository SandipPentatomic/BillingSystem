using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Application.Features.Invoices;

public sealed record PayInvoiceCommand(
    Guid InvoiceId,
    PaymentMode PaymentMode,
    string IdempotencyKey) : ICommand<PayInvoiceResult>, IIdempotentRequest;
