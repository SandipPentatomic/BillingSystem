using SubscriptionBilling.Application.Abstractions.CQRS;

namespace SubscriptionBilling.Application.Features.Billing;

public sealed record RunBillingCycleCommand : ICommand<RunBillingCycleResult>;
