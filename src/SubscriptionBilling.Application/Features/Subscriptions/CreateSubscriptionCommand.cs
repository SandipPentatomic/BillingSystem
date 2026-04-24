using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Application.Features.Subscriptions;

public sealed record CreateSubscriptionCommand(
    Guid CustomerId,
    string PlanName,
    decimal Amount,
    string Currency,
    int BillingInterval,
    BillingIntervalUnit BillingIntervalUnit,
    string IdempotencyKey) : ICommand<CreateSubscriptionResult>, IIdempotentRequest;
