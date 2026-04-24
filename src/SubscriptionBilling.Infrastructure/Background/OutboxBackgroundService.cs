using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubscriptionBilling.Infrastructure.Configuration;
using SubscriptionBilling.Infrastructure.Persistence;

namespace SubscriptionBilling.Infrastructure.Background;

[ExcludeFromCodeCoverage]
public sealed class OutboxBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OutboxBackgroundService> _logger;
    private readonly BillingProcessingOptions _options;

    public OutboxBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OutboxBackgroundService> logger,
        IOptions<BillingProcessingOptions> options)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.OutboxPollingIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

                var pendingMessages = await dbContext.OutboxMessages
                    .Where(message => message.ProcessedOnUtc == null)
                    .OrderBy(message => message.OccurredOnUtc)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                if (pendingMessages.Count == 0)
                {
                    continue;
                }

                foreach (var message in pendingMessages)
                {
                    try
                    {
                        var eventType = Type.GetType(message.Type, throwOnError: false);
                        _logger.LogInformation("Outbox dispatching domain event {EventType}", eventType?.Name ?? message.Type);
                        message.MarkProcessed(DateTime.UtcNow);
                    }
                    catch (Exception exception)
                    {
                        message.MarkFailed(exception.Message);
                        _logger.LogError(exception, "Failed to process outbox message {OutboxMessageId}", message.Id);
                    }
                }

                await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Outbox background job failed.");
            }
        }
    }
}
