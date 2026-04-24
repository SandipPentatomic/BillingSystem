using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SubscriptionBilling.Api.Contracts;
using SubscriptionBilling.Api.Extensions;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Features.Customers;

namespace SubscriptionBilling.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Tags("1. Customers")]
[Produces("application/json")]
public sealed class CustomersController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;

    public CustomersController(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CreateCustomerResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _commandDispatcher.SendAsync(
            new CreateCustomerCommand(request.Name, request.Email, HttpContext.GetIdempotencyKey()),
            cancellationToken);

        return Created($"/api/customers/{result.CustomerId}", result);
    }
}
