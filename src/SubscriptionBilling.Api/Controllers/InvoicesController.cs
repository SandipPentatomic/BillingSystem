using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SubscriptionBilling.Api.Contracts;
using SubscriptionBilling.Api.Headers;
using SubscriptionBilling.Api.Validation;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Features.Invoices;
using SubscriptionBilling.Application.ReadModels;

namespace SubscriptionBilling.Api.Controllers;

[ApiController]
[Route("api/invoices")]
[Tags("3. Invoices")]
[Produces("application/json")]
public sealed class InvoicesController : ControllerBase
{
    private readonly ICommandHandler<PayInvoiceCommand, PayInvoiceResult> _payInvoiceHandler;
    private readonly IQueryHandler<GetInvoicesQuery, PagedResult<InvoiceListItem>> _getInvoicesHandler;

    public InvoicesController(
        ICommandHandler<PayInvoiceCommand, PayInvoiceResult> payInvoiceHandler,
        IQueryHandler<GetInvoicesQuery, PagedResult<InvoiceListItem>> getInvoicesHandler)
    {
        _payInvoiceHandler = payInvoiceHandler;
        _getInvoicesHandler = getInvoicesHandler;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<InvoiceListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] GetInvoicesRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _getInvoicesHandler.HandleAsync(
            new GetInvoicesQuery(
                request.CustomerId,
                request.SubscriptionId,
                request.Status,
                request.PageNumber,
                request.PageSize),
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
        [FromHeader(Name = ApiHeaderNames.IdempotencyKey)] string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var result = await _payInvoiceHandler.HandleAsync(
            new PayInvoiceCommand(invoiceId, request.PaymentMode, IdempotencyKeyGuard.Require(idempotencyKey)),
            cancellationToken);

        return Ok(result);
    }
}
