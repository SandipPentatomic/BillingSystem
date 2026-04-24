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
        var dispatcher = new FakeCommandDispatcher();
        var response = new RunBillingCycleResult(3, 4, DateTime.UtcNow);
        dispatcher.Response = response;
        var controller = new BillingController(dispatcher);

        var result = await controller.RunAsync(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<RunBillingCycleCommand>(dispatcher.LastCommand);
        Assert.Same(response, okResult.Value);
    }
}
