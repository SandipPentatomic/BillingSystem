using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Infrastructure.Persistence.Repositories;
using SubscriptionBilling.Infrastructure.Tests.Support;

namespace SubscriptionBilling.Infrastructure.Tests.Persistence.Repositories;

public sealed class CustomerRepositoryTests
{
    [Fact]
    public async Task AddAsync_And_GetByIdAsync_Persist_Customer()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var repository = new CustomerRepository(dbContext);
        var customer = Customer.Create("Alice", "alice@example.com", DateTime.UtcNow);

        await repository.AddAsync(customer, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var storedCustomer = await repository.GetByIdAsync(customer.Id, CancellationToken.None);

        Assert.NotNull(storedCustomer);
        Assert.Equal(customer.Id, storedCustomer.Id);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Customer_Does_Not_Exist()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var repository = new CustomerRepository(dbContext);

        var customer = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(customer);
    }
}
