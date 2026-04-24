using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SubscriptionBilling.Api.Contracts;
using SubscriptionBilling.Api.Extensions;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Features.Subscriptions;

namespace SubscriptionBilling.Api.Controllers;

[ApiController]
[Route("api/subscriptions")]
[Tags("2. Subscriptions")]
[Produces("application/json")]
public sealed class SubscriptionsController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;

    public SubscriptionsController(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CreateSubscriptionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _commandDispatcher.SendAsync(
            new CreateSubscriptionCommand(
                request.CustomerId,
                request.PlanName,
                request.Amount,
                request.Currency,
                request.BillingInterval,
                request.BillingIntervalUnit,
                HttpContext.GetIdempotencyKey()),
            cancellationToken);

        return Created($"/api/subscriptions/{result.SubscriptionId}", result);
    }

    [HttpPost("{subscriptionId:guid}/cancel")]
    [ProducesResponseType(typeof(CancelSubscriptionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelAsync(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var result = await _commandDispatcher.SendAsync(
            new CancelSubscriptionCommand(subscriptionId, HttpContext.GetIdempotencyKey()),
            cancellationToken);

        return Ok(result);
    }
}
