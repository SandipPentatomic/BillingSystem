using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SubscriptionBilling.Api.Contracts;
using SubscriptionBilling.Api.Extensions;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Features.Invoices;

namespace SubscriptionBilling.Api.Controllers;

[ApiController]
[Route("api/invoices")]
[Tags("3. Invoices")]
[Produces("application/json")]
public sealed class InvoicesController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;

    public InvoicesController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<InvoiceListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? subscriptionId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var result = await _queryDispatcher.QueryAsync(
            new GetInvoicesQuery(customerId, subscriptionId, status),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{invoiceId:guid}/pay")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(PayInvoiceResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PayAsync(
        Guid invoiceId,
        [FromBody] PayInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _commandDispatcher.SendAsync(
            new PayInvoiceCommand(invoiceId, request.PaymentMode, HttpContext.GetIdempotencyKey()),
            cancellationToken);

        return Ok(result);
    }
}
