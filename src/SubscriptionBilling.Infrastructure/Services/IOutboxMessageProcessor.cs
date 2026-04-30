namespace SubscriptionBilling.Infrastructure.Services;

public interface IOutboxMessageProcessor
{
    Task ProcessPendingAsync(CancellationToken cancellationToken);
}
