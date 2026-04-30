using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SubscriptionBilling.Application.Exceptions;
using SubscriptionBilling.Domain.Abstractions;

namespace SubscriptionBilling.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, logLevel) = exception switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Domain validation failed.", LogLevel.Warning),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found.", LogLevel.Information),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Invalid request.", LogLevel.Information),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request.", LogLevel.Warning),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", LogLevel.Error)
        };

        _logger.Log(logLevel, exception, "HTTP {StatusCode} generated for {Path}", statusCode, context.Request.Path);

        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        var detail = statusCode == StatusCodes.Status500InternalServerError
            ? "The server was unable to process the request."
            : exception.Message;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}
