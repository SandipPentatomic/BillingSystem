using Microsoft.AspNetCore.Mvc;
using SubscriptionBilling.Api.Controllers;
using SubscriptionBilling.Api.Tests.Support;
using SubscriptionBilling.Application.Features.Billing;

namespace SubscriptionBilling.Api.Tests.Controllers;

public sealed class BillingControllerTests
{
    [Fact]
    public async Task RunAsync_Dispatches_Command_And_Returns_Ok_Result()
    {
        var handler = new SpyCommandHandler<RunBillingCycleCommand, RunBillingCycleResult>();
        var response = new RunBillingCycleResult(3, 4, DateTime.UtcNow);
        handler.Response = response;
        var controller = new BillingController(handler);

        var result = await controller.RunAsync(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<RunBillingCycleCommand>(handler.LastCommand);
        Assert.Same(response, okResult.Value);
    }
}
