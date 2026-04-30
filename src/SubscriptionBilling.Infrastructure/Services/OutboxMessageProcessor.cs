using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SubscriptionBilling.Application.Abstractions.Clock;
using SubscriptionBilling.Infrastructure.Persistence;

namespace SubscriptionBilling.Infrastructure.Services;

public sealed class OutboxMessageProcessor : IOutboxMessageProcessor
{
    private readonly BillingDbContext _dbContext;
    private readonly IClock _clock;
    private readonly ILogger<OutboxMessageProcessor> _logger;

    public OutboxMessageProcessor(
        BillingDbContext dbContext,
        IClock clock,
        ILogger<OutboxMessageProcessor> logger)
    {
        _dbContext = dbContext;
        _clock = clock;
        _logger = logger;
    }

    public async Task ProcessPendingAsync(CancellationToken cancellationToken)
    {
        var pendingMessages = await _dbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (pendingMessages.Count == 0)
        {
            return;
        }

        foreach (var message in pendingMessages)
        {
            try
            {
                _logger.LogInformation(
                    "Outbox dispatching domain event {EventType}",
                    OutboxEventTypeRegistry.GetDisplayName(message.Type));
                message.MarkProcessed(_clock.UtcNow);
            }
            catch (Exception exception)
            {
                message.MarkFailed(exception.Message);
                _logger.LogError(exception, "Failed to process outbox message {OutboxMessageId}", message.Id);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
