namespace SubscriptionBilling.Application.Features.Customers;

public sealed record CreateCustomerResult(Guid CustomerId, string Name, string Email);
