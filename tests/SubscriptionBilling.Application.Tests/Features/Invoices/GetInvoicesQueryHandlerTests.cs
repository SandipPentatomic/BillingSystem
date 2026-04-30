using SubscriptionBilling.Application.Features.Invoices;
using SubscriptionBilling.Application.ReadModels;
using SubscriptionBilling.Application.Tests.Support;
using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Application.Tests.Features.Invoices;

public sealed class GetInvoicesQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_Delegates_To_Read_Repository()
    {
        var customerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var invoices = new[]
        {
            new InvoiceListItem(
                Guid.NewGuid(),
                customerId,
                subscriptionId,
                59m,
                "USD",
                "Paid",
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(6),
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow,
                PaymentMode.Cash,
                "CASH-REF")
        };

        var repository = new FakeInvoiceReadRepository
        {
            Result = new PagedResult<InvoiceListItem>(invoices, 2, 10, 25)
        };

        var handler = new GetInvoicesQueryHandler(repository);

        var result = await handler.HandleAsync(
            new GetInvoicesQuery(customerId, subscriptionId, InvoiceStatus.Paid, 2, 10),
            CancellationToken.None);

        Assert.Same(invoices, result.Items);
        Assert.Equal(customerId, repository.LastCustomerId);
        Assert.Equal(subscriptionId, repository.LastSubscriptionId);
        Assert.Equal(InvoiceStatus.Paid, repository.LastStatus);
        Assert.Equal(2, repository.LastPageNumber);
        Assert.Equal(10, repository.LastPageSize);
    }
}
