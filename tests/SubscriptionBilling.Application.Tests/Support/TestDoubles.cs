using SubscriptionBilling.Application.Abstractions.Clock;
using SubscriptionBilling.Application.Abstractions.Payments;
using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Application.Features.Invoices;
using SubscriptionBilling.Application.ReadModels;
using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Application.Tests.Support;

internal sealed class FakeClock : IClock
{
    public FakeClock(DateTime utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTime UtcNow { get; set; }
}

internal sealed class SpyUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.FromResult(1);
    }
}

internal sealed class FakePaymentGateway : IPaymentGateway
{
    public ChargePaymentRequest? LastRequest { get; private set; }

    public ChargePaymentResult Result { get; set; } = new("PAYMENT-REFERENCE", DateTime.UtcNow);

    public Task<ChargePaymentResult> ChargeAsync(ChargePaymentRequest request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(Result);
    }
}

internal sealed class FakeCustomerRepository : ICustomerRepository
{
    private readonly Dictionary<Guid, Customer> _customers = [];

    public List<Customer> AddedCustomers { get; } = [];

    public Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        AddedCustomers.Add(customer);
        _customers[customer.Id] = customer;
        return Task.CompletedTask;
    }

    public Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken)
    {
        _customers.TryGetValue(customerId, out var customer);
        return Task.FromResult(customer);
    }

    public void Seed(Customer customer)
    {
        _customers[customer.Id] = customer;
    }
}

internal sealed class FakeSubscriptionRepository : ISubscriptionRepository
{
    private readonly Dictionary<Guid, Subscription> _subscriptions = [];
    private readonly List<Subscription> _dueSubscriptions = [];

    public List<Subscription> AddedSubscriptions { get; } = [];

    public Task AddAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        AddedSubscriptions.Add(subscription);
        _subscriptions[subscription.Id] = subscription;
        return Task.CompletedTask;
    }

    public Task<Subscription?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken)
    {
        _subscriptions.TryGetValue(subscriptionId, out var subscription);
        return Task.FromResult(subscription);
    }

    public Task<IReadOnlyCollection<Subscription>> ListDueForBillingAsync(DateTime asOfUtc, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyCollection<Subscription>>(_dueSubscriptions.ToArray());
    }

    public void Seed(Subscription subscription)
    {
        _subscriptions[subscription.Id] = subscription;
    }

    public void SetDueSubscriptions(params Subscription[] subscriptions)
    {
        _dueSubscriptions.Clear();
        _dueSubscriptions.AddRange(subscriptions);
    }
}

internal sealed class FakeInvoiceRepository : IInvoiceRepository
{
    private readonly Dictionary<Guid, Invoice> _invoices = [];

    public List<Invoice> AddedInvoices { get; } = [];
    public List<IReadOnlyCollection<Invoice>> AddedRanges { get; } = [];

    public Task AddAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        AddedInvoices.Add(invoice);
        _invoices[invoice.Id] = invoice;
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<Invoice> invoices, CancellationToken cancellationToken)
    {
        var invoiceArray = invoices.ToArray();
        AddedRanges.Add(invoiceArray);

        foreach (var invoice in invoiceArray)
        {
            _invoices[invoice.Id] = invoice;
        }

        return Task.CompletedTask;
    }

    public Task<Invoice?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        _invoices.TryGetValue(invoiceId, out var invoice);
        return Task.FromResult(invoice);
    }

    public void Seed(Invoice invoice)
    {
        _invoices[invoice.Id] = invoice;
    }
}

internal sealed class FakeInvoiceReadRepository : IInvoiceReadRepository
{
    public PagedResult<InvoiceListItem> Result { get; set; } = new([], 1, 50, 0);

    public Guid? LastCustomerId { get; private set; }
    public Guid? LastSubscriptionId { get; private set; }
    public InvoiceStatus? LastStatus { get; private set; }
    public int LastPageNumber { get; private set; }
    public int LastPageSize { get; private set; }

    public Task<PagedResult<InvoiceListItem>> ListAsync(
        Guid? customerId,
        Guid? subscriptionId,
        InvoiceStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        LastCustomerId = customerId;
        LastSubscriptionId = subscriptionId;
        LastStatus = status;
        LastPageNumber = pageNumber;
        LastPageSize = pageSize;
        return Task.FromResult(Result);
    }
}
