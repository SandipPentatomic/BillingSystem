using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SubscriptionBilling.Application.Abstractions.Clock;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Infrastructure;
using SubscriptionBilling.Infrastructure.Background;
using SubscriptionBilling.Infrastructure.Configuration;
using SubscriptionBilling.Infrastructure.Persistence;

namespace SubscriptionBilling.Infrastructure.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_Registers_Core_Services()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{BillingProcessingOptions.SectionName}:BillingCyclePollingIntervalSeconds"] = "3",
                [$"{BillingProcessingOptions.SectionName}:OutboxPollingIntervalSeconds"] = "4"
            })
            .Build();

        var services = new ServiceCollection();

        services.AddInfrastructure(configuration);

        using var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetService<BillingDbContext>());
        Assert.NotNull(serviceProvider.GetService<IClock>());
        Assert.NotNull(serviceProvider.GetService<ICommandDispatcher>());
        Assert.NotNull(serviceProvider.GetService<IQueryDispatcher>());
        Assert.NotNull(serviceProvider.GetService<IUnitOfWork>());
        Assert.NotNull(serviceProvider.GetService<IIdempotencyStore>());
        Assert.NotNull(serviceProvider.GetService<ICustomerRepository>());
        Assert.NotNull(serviceProvider.GetService<ISubscriptionRepository>());
        Assert.NotNull(serviceProvider.GetService<IInvoiceRepository>());
        Assert.NotNull(serviceProvider.GetService<IInvoiceReadRepository>());

        var hostedServices = services
            .Where(descriptor => descriptor.ServiceType == typeof(IHostedService))
            .Select(descriptor => descriptor.ImplementationType)
            .ToArray();

        Assert.Contains(typeof(BillingCycleBackgroundService), hostedServices);
        Assert.Contains(typeof(OutboxBackgroundService), hostedServices);
    }
}
