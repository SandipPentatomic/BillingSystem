using Microsoft.Extensions.DependencyInjection;
using SubscriptionBilling.Application;
using SubscriptionBilling.Application.Abstractions.Clock;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Application.Features.Billing;
using SubscriptionBilling.Application.Features.Customers;
using SubscriptionBilling.Application.Features.Invoices;
using SubscriptionBilling.Application.Features.Subscriptions;
using SubscriptionBilling.Application.Tests.Support;

namespace SubscriptionBilling.Application.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_Registers_Command_And_Query_Handlers()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IClock>(new FakeClock(DateTime.UtcNow));
        services.AddSingleton<ICustomerRepository, FakeCustomerRepository>();
        services.AddSingleton<ISubscriptionRepository, FakeSubscriptionRepository>();
        services.AddSingleton<IInvoiceRepository, FakeInvoiceRepository>();
        services.AddSingleton<IInvoiceReadRepository, FakeInvoiceReadRepository>();
        services.AddSingleton<IUnitOfWork, SpyUnitOfWork>();

        services.AddApplication();

        using var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetService<ICommandHandler<CreateCustomerCommand, CreateCustomerResult>>());
        Assert.NotNull(serviceProvider.GetService<ICommandHandler<CreateSubscriptionCommand, CreateSubscriptionResult>>());
        Assert.NotNull(serviceProvider.GetService<ICommandHandler<CancelSubscriptionCommand, CancelSubscriptionResult>>());
        Assert.NotNull(serviceProvider.GetService<ICommandHandler<PayInvoiceCommand, PayInvoiceResult>>());
        Assert.NotNull(serviceProvider.GetService<ICommandHandler<RunBillingCycleCommand, RunBillingCycleResult>>());
        Assert.NotNull(serviceProvider.GetService<IQueryHandler<GetInvoicesQuery, IReadOnlyCollection<InvoiceListItem>>>());
    }
}
