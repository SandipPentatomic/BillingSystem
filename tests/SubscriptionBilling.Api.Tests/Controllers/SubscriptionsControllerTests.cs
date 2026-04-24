using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SubscriptionBilling.Api.Contracts;
using SubscriptionBilling.Api.Controllers;
using SubscriptionBilling.Api.Tests.Support;
using SubscriptionBilling.Application.Features.Subscriptions;
using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Api.Tests.Controllers;

public sealed class SubscriptionsControllerTests
{
    [Fact]
    public async Task CreateAsync_Dispatches_Command_And_Returns_Created_Result()
    {
        var dispatcher = new FakeCommandDispatcher();
        var customerId = Guid.NewGuid();
        var response = new CreateSubscriptionResult(
            Guid.NewGuid(),
            customerId,
            "Growth",
            "Active",
            new DateTime(2026, 4, 24, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 24, 10, 15, 0, DateTimeKind.Utc),
            Guid.NewGuid(),
            59m,
            "USD",
            15,
            BillingIntervalUnit.Minutes);
        dispatcher.Response = response;

        var controller = new SubscriptionsController(dispatcher)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Request.Headers["Idempotency-Key"] = "subscription-key";

        var request = new CreateSubscriptionRequest(customerId, "Growth", 59m, "USD", 15, BillingIntervalUnit.Minutes);

        var result = await controller.CreateAsync(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedResult>(result);
        var command = Assert.IsType<CreateSubscriptionCommand>(dispatcher.LastCommand);

        Assert.Equal(customerId, command.CustomerId);
        Assert.Equal("subscription-key", command.IdempotencyKey);
        Assert.Equal(BillingIntervalUnit.Minutes, command.BillingIntervalUnit);
        Assert.Equal($"/api/subscriptions/{response.SubscriptionId}", createdResult.Location);
        Assert.Same(response, createdResult.Value);
    }

    [Fact]
    public async Task CancelAsync_Dispatches_Command_And_Returns_Ok_Result()
    {
        var dispatcher = new FakeCommandDispatcher();
        var subscriptionId = Guid.NewGuid();
        var response = new CancelSubscriptionResult(subscriptionId, "Cancelled", DateTime.UtcNow);
        dispatcher.Response = response;

        var controller = new SubscriptionsController(dispatcher)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Request.Headers["Idempotency-Key"] = "cancel-key";

        var result = await controller.CancelAsync(subscriptionId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var command = Assert.IsType<CancelSubscriptionCommand>(dispatcher.LastCommand);

        Assert.Equal(subscriptionId, command.SubscriptionId);
        Assert.Equal("cancel-key", command.IdempotencyKey);
        Assert.Same(response, okResult.Value);
    }
}
