using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SubscriptionBilling.Api.Contracts;
using SubscriptionBilling.Api.Controllers;
using SubscriptionBilling.Api.Tests.Support;
using SubscriptionBilling.Application.Features.Customers;

namespace SubscriptionBilling.Api.Tests.Controllers;

public sealed class CustomersControllerTests
{
    [Fact]
    public async Task CreateAsync_Dispatches_Command_And_Returns_Created_Result()
    {
        var handler = new SpyCommandHandler<CreateCustomerCommand, CreateCustomerResult>
        {
            Response = new CreateCustomerResult(Guid.NewGuid(), "Alice", "alice@example.com")
        };

        var controller = new CustomersController(handler);

        var result = await controller.CreateAsync(new CreateCustomerRequest("Alice", "alice@example.com"), "customer-key", CancellationToken.None);

        var createdResult = Assert.IsType<CreatedResult>(result);
        var command = Assert.IsType<CreateCustomerCommand>(handler.LastCommand);

        Assert.Equal("customer-key", command.IdempotencyKey);
        Assert.Equal("Alice", command.Name);
        Assert.Equal("alice@example.com", command.Email);
        Assert.Equal($"/api/customers/{handler.Response.CustomerId}", createdResult.Location);
        Assert.Same(handler.Response, createdResult.Value);
    }

    [Fact]
    public async Task CreateAsync_Throws_When_Idempotency_Header_Is_Missing()
    {
        var controller = new CustomersController(new SpyCommandHandler<CreateCustomerCommand, CreateCustomerResult>());

        var exception = await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            controller.CreateAsync(new CreateCustomerRequest("Alice", "alice@example.com"), string.Empty, CancellationToken.None));

        Assert.Equal("The Idempotency-Key header is required for this operation.", exception.Message);
    }
}
