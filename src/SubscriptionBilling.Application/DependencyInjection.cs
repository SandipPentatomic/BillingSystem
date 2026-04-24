using Microsoft.Extensions.DependencyInjection;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Features.Billing;
using SubscriptionBilling.Application.Features.Customers;
using SubscriptionBilling.Application.Features.Invoices;
using SubscriptionBilling.Application.Features.Subscriptions;

namespace SubscriptionBilling.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateCustomerCommand, CreateCustomerResult>, CreateCustomerCommandHandler>();
        services.AddScoped<ICommandHandler<CreateSubscriptionCommand, CreateSubscriptionResult>, CreateSubscriptionCommandHandler>();
        services.AddScoped<ICommandHandler<CancelSubscriptionCommand, CancelSubscriptionResult>, CancelSubscriptionCommandHandler>();
        services.AddScoped<ICommandHandler<PayInvoiceCommand, PayInvoiceResult>, PayInvoiceCommandHandler>();
        services.AddScoped<ICommandHandler<RunBillingCycleCommand, RunBillingCycleResult>, RunBillingCycleCommandHandler>();

        services.AddScoped<IQueryHandler<GetInvoicesQuery, IReadOnlyCollection<InvoiceListItem>>, GetInvoicesQueryHandler>();

        return services;
    }
}
