using System.Text.Json;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Infrastructure.Persistence;

namespace SubscriptionBilling.Infrastructure.Services;

public sealed class IdempotentCommandHandlerDecorator<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>, IIdempotentRequest
{
    private readonly ICommandHandler<TCommand, TResponse> _innerHandler;
    private readonly IIdempotencyStore _idempotencyStore;

    public IdempotentCommandHandlerDecorator(
        ICommandHandler<TCommand, TResponse> innerHandler,
        IIdempotencyStore idempotencyStore)
    {
        _innerHandler = innerHandler;
        _idempotencyStore = idempotencyStore;
    }

    public async Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        await using var idempotencyLease = await _idempotencyStore.AcquireAsync(command.IdempotencyKey, cancellationToken);

        var cachedResponse = await _idempotencyStore.GetResponseAsync(command.IdempotencyKey, cancellationToken);

        if (!string.IsNullOrWhiteSpace(cachedResponse))
        {
            var deserializedResponse = JsonSerializer.Deserialize<TResponse>(cachedResponse, JsonDefaults.Options);

            return deserializedResponse
                   ?? throw new InvalidOperationException($"Cached idempotent response for '{typeof(TCommand).Name}' could not be deserialized.");
        }

        var response = await _innerHandler.HandleAsync(command, cancellationToken);

        await _idempotencyStore.SaveResponseAsync(
            command.IdempotencyKey,
            JsonSerializer.Serialize(response, JsonDefaults.Options),
            cancellationToken);

        return response;
    }
}
