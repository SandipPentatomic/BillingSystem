using SubscriptionBilling.Application.Abstractions.CQRS;

namespace SubscriptionBilling.Api.Tests.Support;

internal sealed class SpyCommandHandler<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public TCommand? LastCommand { get; private set; }

    public TResponse Response { get; set; } = default!;

    public Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        LastCommand = command;
        return Task.FromResult(Response);
    }
}

internal sealed class SpyQueryHandler<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public TQuery? LastQuery { get; private set; }

    public TResponse Response { get; set; } = default!;

    public Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken)
    {
        LastQuery = query;
        return Task.FromResult(Response);
    }
}
