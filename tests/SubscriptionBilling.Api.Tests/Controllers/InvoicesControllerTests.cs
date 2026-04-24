using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SubscriptionBilling.Api.Contracts;
using SubscriptionBilling.Api.Controllers;
using SubscriptionBilling.Api.Tests.Support;
using SubscriptionBilling.Application.Features.Invoices;
using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Api.Tests.Controllers;

public sealed class InvoicesControllerTests
{
    [Fact]
    public async Task GetAsync_Dispatches_Query_And_Returns_Ok_Result()
    {
        var customerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var queryDispatcher = new FakeQueryDispatcher();
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
                null)
        };

        queryDispatcher.Response = invoices;

        var controller = new InvoicesController(new FakeCommandDispatcher(), queryDispatcher);

        var result = await controller.GetAsync(customerId, subscriptionId, "Pending", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var query = Assert.IsType<GetInvoicesQuery>(queryDispatcher.LastQuery);

        Assert.Equal(customerId, query.CustomerId);
        Assert.Equal(subscriptionId, query.SubscriptionId);
        Assert.Equal("Pending", query.Status);
        Assert.Same(invoices, okResult.Value);
    }

    [Fact]
    public async Task PayAsync_Dispatches_Command_And_Returns_Ok_Result()
    {
        var commandDispatcher = new FakeCommandDispatcher();
        var queryDispatcher = new FakeQueryDispatcher();
        var invoiceId = Guid.NewGuid();
        var response = new PayInvoiceResult(invoiceId, "Paid", DateTime.UtcNow, PaymentMode.Online);
        commandDispatcher.Response = response;

        var controller = new InvoicesController(commandDispatcher, queryDispatcher)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Request.Headers["Idempotency-Key"] = "invoice-key";

        var result = await controller.PayAsync(invoiceId, new PayInvoiceRequest(PaymentMode.Online), CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var command = Assert.IsType<PayInvoiceCommand>(commandDispatcher.LastCommand);

        Assert.Equal(invoiceId, command.InvoiceId);
        Assert.Equal(PaymentMode.Online, command.PaymentMode);
        Assert.Equal("invoice-key", command.IdempotencyKey);
        Assert.Same(response, okResult.Value);
    }
}
