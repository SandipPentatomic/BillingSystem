using Microsoft.EntityFrameworkCore;
using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Domain.Aggregates;

namespace SubscriptionBilling.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly BillingDbContext _dbContext;

    public CustomerRepository(BillingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        return _dbContext.Customers.AddAsync(customer, cancellationToken).AsTask();
    }

    public Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken)
    {
        return _dbContext.Customers.SingleOrDefaultAsync(customer => customer.Id == customerId, cancellationToken);
    }
}
