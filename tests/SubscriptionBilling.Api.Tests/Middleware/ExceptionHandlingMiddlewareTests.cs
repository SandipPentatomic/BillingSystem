using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using SubscriptionBilling.Api.Middleware;
using SubscriptionBilling.Application.Exceptions;
using SubscriptionBilling.Domain.Abstractions;

namespace SubscriptionBilling.Api.Tests.Middleware;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_Maps_DomainException_To_BadRequest_ProblemDetails()
    {
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new DomainException("Invoice cannot be paid twice."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        var payload = await ReadPayloadAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        Assert.Equal("Domain validation failed.", payload.RootElement.GetProperty("title").GetString());
        Assert.Equal("Invoice cannot be paid twice.", payload.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task InvokeAsync_Maps_NotFoundException_To_NotFound_ProblemDetails()
    {
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new NotFoundException("Invoice '123' was not found."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        var payload = await ReadPayloadAsync(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Equal("Resource not found.", payload.RootElement.GetProperty("title").GetString());
        Assert.Equal("Invoice '123' was not found.", payload.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task InvokeAsync_Maps_Unexpected_Exception_To_InternalServerError_ProblemDetails()
    {
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("Unexpected failure."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        var payload = await ReadPayloadAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("An unexpected error occurred.", payload.RootElement.GetProperty("title").GetString());
        Assert.Equal("The server was unable to process the request.", payload.RootElement.GetProperty("detail").GetString());
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        return new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };
    }

    private static async Task<JsonDocument> ReadPayloadAsync(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(context.Response.Body);
    }
}
