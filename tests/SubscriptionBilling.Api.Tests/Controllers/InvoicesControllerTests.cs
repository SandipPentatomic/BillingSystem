using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SubscriptionBilling.Api.Contracts;
using SubscriptionBilling.Api.Controllers;
using SubscriptionBilling.Api.Tests.Support;
using SubscriptionBilling.Application.Features.Invoices;
using SubscriptionBilling.Application.ReadModels;
using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Api.Tests.Controllers;

public sealed class InvoicesControllerTests
{
    [Fact]
    public async Task GetAsync_Dispatches_Query_And_Returns_Ok_Result()
    {
        var customerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var payHandler = new SpyCommandHandler<PayInvoiceCommand, PayInvoiceResult>();
        var queryHandler = new SpyQueryHandler<GetInvoicesQuery, PagedResult<InvoiceListItem>>();
        var invoices = new[]
        {
            new InvoiceListItem(
                Guid.NewGuid(),
                customerId,
                subscriptionId,
                59m,
                "USD",
                "Pending",
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(7),
                DateTime.UtcNow,
                null,
                null,
                null)
        };

        queryHandler.Response = new PagedResult<InvoiceListItem>(invoices, 3, 5, 21);

        var controller = new InvoicesController(payHandler, queryHandler);

        var result = await controller.GetAsync(
            new GetInvoicesRequest(customerId, subscriptionId, InvoiceStatus.Pending, 3, 5),
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var query = Assert.IsType<GetInvoicesQuery>(queryHandler.LastQuery);
        var response = Assert.IsType<PagedResult<InvoiceListItem>>(okResult.Value);

        Assert.Equal(customerId, query.CustomerId);
        Assert.Equal(subscriptionId, query.SubscriptionId);
        Assert.Equal(InvoiceStatus.Pending, query.Status);
        Assert.Equal(3, query.PageNumber);
        Assert.Equal(5, query.PageSize);
        Assert.Same(queryHandler.Response, response);
    }

    [Fact]
    public async Task PayAsync_Dispatches_Command_And_Returns_Ok_Result()
    {
        var payHandler = new SpyCommandHandler<PayInvoiceCommand, PayInvoiceResult>();
        var queryHandler = new SpyQueryHandler<GetInvoicesQuery, PagedResult<InvoiceListItem>>();
        var invoiceId = Guid.NewGuid();
        var response = new PayInvoiceResult(invoiceId, "Paid", DateTime.UtcNow, PaymentMode.Online, "ONLINE-REF-001");
        payHandler.Response = response;

        var controller = new InvoicesController(payHandler, queryHandler);

        var result = await controller.PayAsync(invoiceId, new PayInvoiceRequest(PaymentMode.Online), "invoice-key", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var command = Assert.IsType<PayInvoiceCommand>(payHandler.LastCommand);

        Assert.Equal(invoiceId, command.InvoiceId);
        Assert.Equal(PaymentMode.Online, command.PaymentMode);
        Assert.Equal("invoice-key", command.IdempotencyKey);
        Assert.Same(response, okResult.Value);
    }

    [Fact]
    public async Task PayAsync_Throws_When_Idempotency_Header_Is_Missing()
    {
        var controller = new InvoicesController(
            new SpyCommandHandler<PayInvoiceCommand, PayInvoiceResult>(),
            new SpyQueryHandler<GetInvoicesQuery, PagedResult<InvoiceListItem>>());

        var exception = await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            controller.PayAsync(Guid.NewGuid(), new PayInvoiceRequest(PaymentMode.Online), string.Empty, CancellationToken.None));

        Assert.Equal("The Idempotency-Key header is required for this operation.", exception.Message);
    }
}
