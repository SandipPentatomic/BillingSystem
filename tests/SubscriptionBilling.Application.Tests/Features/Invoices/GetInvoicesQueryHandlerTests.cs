using SubscriptionBilling.Application.Features.Invoices;
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
                PaymentMode.Cash)
        };

        var repository = new FakeInvoiceReadRepository
        {
            Result = invoices
        };

        var handler = new GetInvoicesQueryHandler(repository);

        var result = await handler.HandleAsync(new GetInvoicesQuery(customerId, subscriptionId, "Paid"), CancellationToken.None);

        Assert.Same(invoices, result);
        Assert.Equal(customerId, repository.LastCustomerId);
        Assert.Equal(subscriptionId, repository.LastSubscriptionId);
        Assert.Equal("Paid", repository.LastStatus);
    }
}
