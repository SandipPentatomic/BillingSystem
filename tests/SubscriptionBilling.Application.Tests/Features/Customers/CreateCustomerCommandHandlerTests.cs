using SubscriptionBilling.Application.Features.Customers;
using SubscriptionBilling.Application.Tests.Support;

namespace SubscriptionBilling.Application.Tests.Features.Customers;

public sealed class CreateCustomerCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Creates_Customer_And_Persists_Changes()
    {
        var now = new DateTime(2026, 4, 24, 10, 0, 0, DateTimeKind.Utc);
        var clock = new FakeClock(now);
        var customerRepository = new FakeCustomerRepository();
        var unitOfWork = new SpyUnitOfWork();
        var handler = new CreateCustomerCommandHandler(clock, customerRepository, unitOfWork);

        var result = await handler.HandleAsync(
            new CreateCustomerCommand("  Alice  ", "alice@example.com", "customer-key"),
            CancellationToken.None);

        var addedCustomer = Assert.Single(customerRepository.AddedCustomers);
        Assert.Equal(addedCustomer.Id, result.CustomerId);
        Assert.Equal("Alice", result.Name);
        Assert.Equal("alice@example.com", result.Email);
        Assert.Equal(now, addedCustomer.CreatedOnUtc);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }
}
