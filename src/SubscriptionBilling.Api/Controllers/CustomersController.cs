using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SubscriptionBilling.Api.Contracts;
using SubscriptionBilling.Api.Headers;
using SubscriptionBilling.Api.Validation;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Features.Customers;

namespace SubscriptionBilling.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Tags("1. Customers")]
[Produces("application/json")]
public sealed class CustomersController : ControllerBase
{
    private readonly ICommandHandler<CreateCustomerCommand, CreateCustomerResult> _createCustomerHandler;

    public CustomersController(ICommandHandler<CreateCustomerCommand, CreateCustomerResult> createCustomerHandler)
    {
        _createCustomerHandler = createCustomerHandler;
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CreateCustomerResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateCustomerRequest request,
        [FromHeader(Name = ApiHeaderNames.IdempotencyKey)] string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var result = await _createCustomerHandler.HandleAsync(
            new CreateCustomerCommand(request.Name, request.Email, IdempotencyKeyGuard.Require(idempotencyKey)),
            cancellationToken);

        return Created($"/api/customers/{result.CustomerId}", result);
    }
}
