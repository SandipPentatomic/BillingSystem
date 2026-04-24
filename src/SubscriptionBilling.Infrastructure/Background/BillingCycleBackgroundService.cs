using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Features.Billing;
using SubscriptionBilling.Infrastructure.Configuration;

namespace SubscriptionBilling.Infrastructure.Background;

[ExcludeFromCodeCoverage]
public sealed class BillingCycleBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<BillingCycleBackgroundService> _logger;
    private readonly BillingProcessingOptions _options;

    public BillingCycleBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<BillingCycleBackgroundService> logger,
        IOptions<BillingProcessingOptions> options)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.BillingCyclePollingIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var commandDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();
                var result = await commandDispatcher.SendAsync(new RunBillingCycleCommand(), stoppingToken);

                if (result.InvoicesGenerated > 0)
                {
                    _logger.LogInformation(
                        "Billing cycle completed. Subscriptions processed: {SubscriptionsProcessed}, invoices generated: {InvoicesGenerated}",
                        result.SubscriptionsProcessed,
                        result.InvoicesGenerated);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Billing cycle background job failed.");
            }
        }
    }
}
