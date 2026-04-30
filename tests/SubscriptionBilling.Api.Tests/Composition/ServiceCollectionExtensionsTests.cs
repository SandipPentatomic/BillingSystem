using Microsoft.Extensions.DependencyInjection;
using SubscriptionBilling.Api.Composition;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Features.Billing;
using SubscriptionBilling.Application.Features.Customers;
using SubscriptionBilling.Application.Features.Invoices;
using SubscriptionBilling.Application.ReadModels;
using SubscriptionBilling.Application.Features.Subscriptions;

namespace SubscriptionBilling.Api.Tests.Composition;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddUseCaseHandlers_Registers_Explicit_Command_And_Query_Handlers()
    {
        var services = new ServiceCollection();

        services.AddUseCaseHandlers();

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(CreateCustomerCommandHandler));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(CreateSubscriptionCommandHandler));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(CancelSubscriptionCommandHandler));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(PayInvoiceCommandHandler));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(RunBillingCycleCommandHandler));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(GetInvoicesQueryHandler));

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ICommandHandler<CreateCustomerCommand, CreateCustomerResult>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ICommandHandler<CreateSubscriptionCommand, CreateSubscriptionResult>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ICommandHandler<CancelSubscriptionCommand, CancelSubscriptionResult>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ICommandHandler<PayInvoiceCommand, PayInvoiceResult>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ICommandHandler<RunBillingCycleCommand, RunBillingCycleResult>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IQueryHandler<GetInvoicesQuery, PagedResult<InvoiceListItem>>));
    }
}
