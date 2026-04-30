using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SubscriptionBilling.Api.Contracts;
using SubscriptionBilling.Api.Headers;
using SubscriptionBilling.Api.Validation;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Features.Subscriptions;

namespace SubscriptionBilling.Api.Controllers;

[ApiController]
[Route("api/subscriptions")]
[Tags("2. Subscriptions")]
[Produces("application/json")]
public sealed class SubscriptionsController : ControllerBase
{
    private readonly ICommandHandler<CreateSubscriptionCommand, CreateSubscriptionResult> _createSubscriptionHandler;
    private readonly ICommandHandler<CancelSubscriptionCommand, CancelSubscriptionResult> _cancelSubscriptionHandler;

    public SubscriptionsController(
        ICommandHandler<CreateSubscriptionCommand, CreateSubscriptionResult> createSubscriptionHandler,
        ICommandHandler<CancelSubscriptionCommand, CancelSubscriptionResult> cancelSubscriptionHandler)
    {
        _createSubscriptionHandler = createSubscriptionHandler;
        _cancelSubscriptionHandler = cancelSubscriptionHandler;
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CreateSubscriptionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateSubscriptionRequest request,
        [FromHeader(Name = ApiHeaderNames.IdempotencyKey)] string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var result = await _createSubscriptionHandler.HandleAsync(
            new CreateSubscriptionCommand(
                request.CustomerId,
                request.PlanName,
                request.Amount,
                request.Currency,
                request.BillingInterval,
                request.BillingIntervalUnit,
                IdempotencyKeyGuard.Require(idempotencyKey)),
            cancellationToken);

        return Created($"/api/subscriptions/{result.SubscriptionId}", result);
    }

    [HttpPost("{subscriptionId:guid}/cancel")]
    [ProducesResponseType(typeof(CancelSubscriptionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelAsync(
        Guid subscriptionId,
        [FromHeader(Name = ApiHeaderNames.IdempotencyKey)] string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var result = await _cancelSubscriptionHandler.HandleAsync(
            new CancelSubscriptionCommand(subscriptionId, IdempotencyKeyGuard.Require(idempotencyKey)),
            cancellationToken);

        return Ok(result);
    }
}
