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
        var dispatcher = new FakeCommandDispatcher
        {
            Response = new CreateCustomerResult(Guid.NewGuid(), "Alice", "alice@example.com")
        };

        var controller = new CustomersController(dispatcher)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Request.Headers["Idempotency-Key"] = "customer-key";

        var result = await controller.CreateAsync(new CreateCustomerRequest("Alice", "alice@example.com"), CancellationToken.None);

        var createdResult = Assert.IsType<CreatedResult>(result);
        var command = Assert.IsType<CreateCustomerCommand>(dispatcher.LastCommand);

        Assert.Equal("customer-key", command.IdempotencyKey);
        Assert.Equal("Alice", command.Name);
        Assert.Equal("alice@example.com", command.Email);
        Assert.Equal($"/api/customers/{((CreateCustomerResult)dispatcher.Response).CustomerId}", createdResult.Location);
        Assert.Same(dispatcher.Response, createdResult.Value);
    }
}
