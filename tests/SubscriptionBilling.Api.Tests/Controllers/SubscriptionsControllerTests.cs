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
        var createHandler = new SpyCommandHandler<CreateSubscriptionCommand, CreateSubscriptionResult>();
        var cancelHandler = new SpyCommandHandler<CancelSubscriptionCommand, CancelSubscriptionResult>();
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
        createHandler.Response = response;

        var controller = new SubscriptionsController(createHandler, cancelHandler);

        var request = new CreateSubscriptionRequest(customerId, "Growth", 59m, "USD", 15, BillingIntervalUnit.Minutes);

        var result = await controller.CreateAsync(request, "subscription-key", CancellationToken.None);

        var createdResult = Assert.IsType<CreatedResult>(result);
        var command = Assert.IsType<CreateSubscriptionCommand>(createHandler.LastCommand);

        Assert.Equal(customerId, command.CustomerId);
        Assert.Equal("subscription-key", command.IdempotencyKey);
        Assert.Equal(BillingIntervalUnit.Minutes, command.BillingIntervalUnit);
        Assert.Equal($"/api/subscriptions/{response.SubscriptionId}", createdResult.Location);
        Assert.Same(response, createdResult.Value);
    }

    [Fact]
    public async Task CancelAsync_Dispatches_Command_And_Returns_Ok_Result()
    {
        var createHandler = new SpyCommandHandler<CreateSubscriptionCommand, CreateSubscriptionResult>();
        var cancelHandler = new SpyCommandHandler<CancelSubscriptionCommand, CancelSubscriptionResult>();
        var subscriptionId = Guid.NewGuid();
        var response = new CancelSubscriptionResult(subscriptionId, "Cancelled", DateTime.UtcNow);
        cancelHandler.Response = response;

        var controller = new SubscriptionsController(createHandler, cancelHandler);

        var result = await controller.CancelAsync(subscriptionId, "cancel-key", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var command = Assert.IsType<CancelSubscriptionCommand>(cancelHandler.LastCommand);

        Assert.Equal(subscriptionId, command.SubscriptionId);
        Assert.Equal("cancel-key", command.IdempotencyKey);
        Assert.Same(response, okResult.Value);
    }

    [Fact]
    public async Task CancelAsync_Throws_When_Idempotency_Header_Is_Missing()
    {
        var controller = new SubscriptionsController(
            new SpyCommandHandler<CreateSubscriptionCommand, CreateSubscriptionResult>(),
            new SpyCommandHandler<CancelSubscriptionCommand, CancelSubscriptionResult>());

        var exception = await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            controller.CancelAsync(Guid.NewGuid(), string.Empty, CancellationToken.None));

        Assert.Equal("The Idempotency-Key header is required for this operation.", exception.Message);
    }
}
