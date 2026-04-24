using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Infrastructure.Persistence;

namespace SubscriptionBilling.Infrastructure.Services;

public sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IIdempotencyStore _idempotencyStore;

    public CommandDispatcher(IServiceProvider serviceProvider, IIdempotencyStore idempotencyStore)
    {
        _serviceProvider = serviceProvider;
        _idempotencyStore = idempotencyStore;
    }

    public async Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        if (command is IIdempotentRequest idempotentRequest)
        {
            var cachedResponse = await _idempotencyStore.GetResponseAsync(idempotentRequest.IdempotencyKey, cancellationToken);

            if (!string.IsNullOrWhiteSpace(cachedResponse))
            {
                return JsonSerializer.Deserialize<TResponse>(cachedResponse, JsonDefaults.Options)!;
            }
        }

        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        TResponse response = await handler.HandleAsync((dynamic)command, cancellationToken);

        if (command is IIdempotentRequest request)
        {
            await _idempotencyStore.SaveResponseAsync(
                request.IdempotencyKey,
                JsonSerializer.Serialize(response, JsonDefaults.Options),
                cancellationToken);
        }

        return response;
    }
}
