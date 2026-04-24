namespace SubscriptionBilling.Application.Abstractions.CQRS;

public interface ICommandDispatcher
{
    Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
}
