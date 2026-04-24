using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Features.Billing;

namespace SubscriptionBilling.Api.Controllers;

[ApiController]
[Route("api/billing")]
[Tags("4. Billing")]
[Produces("application/json")]
public sealed class BillingController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;

    public BillingController(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    [HttpPost("run")]
    [ProducesResponseType(typeof(RunBillingCycleResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> RunAsync(CancellationToken cancellationToken)
    {
        var result = await _commandDispatcher.SendAsync(new RunBillingCycleCommand(), cancellationToken);
        return Ok(result);
    }
}
