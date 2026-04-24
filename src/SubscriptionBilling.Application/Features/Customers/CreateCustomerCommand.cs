using SubscriptionBilling.Application.Abstractions.CQRS;

namespace SubscriptionBilling.Application.Features.Customers;

public sealed record CreateCustomerCommand(
    string Name,
    string Email,
    string IdempotencyKey) : ICommand<CreateCustomerResult>, IIdempotentRequest;
