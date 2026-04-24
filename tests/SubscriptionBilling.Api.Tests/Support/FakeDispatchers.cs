using SubscriptionBilling.Application.Abstractions.CQRS;

namespace SubscriptionBilling.Api.Tests.Support;

internal sealed class FakeCommandDispatcher : ICommandDispatcher
{
    public object? LastCommand { get; private set; }
    public object? Response { get; set; }

    public Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        LastCommand = command;
        return Task.FromResult((TResponse)Response!);
    }
}

internal sealed class FakeQueryDispatcher : IQueryDispatcher
{
    public object? LastQuery { get; private set; }
    public object? Response { get; set; }

    public Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        LastQuery = query;
        return Task.FromResult((TResponse)Response!);
    }
}
