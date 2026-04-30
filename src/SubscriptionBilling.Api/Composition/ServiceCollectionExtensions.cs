using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Application.Features.Billing;
using SubscriptionBilling.Application.Features.Customers;
using SubscriptionBilling.Application.Features.Invoices;
using SubscriptionBilling.Application.ReadModels;
using SubscriptionBilling.Application.Features.Subscriptions;
using SubscriptionBilling.Infrastructure.Services;

namespace SubscriptionBilling.Api.Composition;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUseCaseHandlers(this IServiceCollection services)
    {
        services.AddIdempotentCommandHandler<CreateCustomerCommand, CreateCustomerResult, CreateCustomerCommandHandler>();
        services.AddIdempotentCommandHandler<CreateSubscriptionCommand, CreateSubscriptionResult, CreateSubscriptionCommandHandler>();
        services.AddIdempotentCommandHandler<CancelSubscriptionCommand, CancelSubscriptionResult, CancelSubscriptionCommandHandler>();
        services.AddIdempotentCommandHandler<PayInvoiceCommand, PayInvoiceResult, PayInvoiceCommandHandler>();

        services.AddCommandHandler<RunBillingCycleCommand, RunBillingCycleResult, RunBillingCycleCommandHandler>();
        services.AddQueryHandler<GetInvoicesQuery, PagedResult<InvoiceListItem>, GetInvoicesQueryHandler>();

        return services;
    }

    private static IServiceCollection AddCommandHandler<TCommand, TResponse, THandler>(this IServiceCollection services)
        where TCommand : class, ICommand<TResponse>
        where THandler : class, ICommandHandler<TCommand, TResponse>
    {
        services.AddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand, TResponse>>(serviceProvider => serviceProvider.GetRequiredService<THandler>());

        return services;
    }

    private static IServiceCollection AddIdempotentCommandHandler<TCommand, TResponse, THandler>(this IServiceCollection services)
        where TCommand : class, ICommand<TResponse>, IIdempotentRequest
        where THandler : class, ICommandHandler<TCommand, TResponse>
    {
        services.AddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand, TResponse>>(serviceProvider =>
            new IdempotentCommandHandlerDecorator<TCommand, TResponse>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IIdempotencyStore>()));

        return services;
    }

    private static IServiceCollection AddQueryHandler<TQuery, TResponse, THandler>(this IServiceCollection services)
        where TQuery : class, IQuery<TResponse>
        where THandler : class, IQueryHandler<TQuery, TResponse>
    {
        services.AddScoped<THandler>();
        services.AddScoped<IQueryHandler<TQuery, TResponse>>(serviceProvider => serviceProvider.GetRequiredService<THandler>());

        return services;
    }
}
