using SubscriptionBilling.Application.Abstractions.Clock;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Application.Exceptions;
using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Application.Features.Subscriptions;

public sealed class CreateSubscriptionCommandHandler : ICommandHandler<CreateSubscriptionCommand, CreateSubscriptionResult>
{
    private readonly IClock _clock;
    private readonly ICustomerRepository _customerRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSubscriptionCommandHandler(
        IClock clock,
        ICustomerRepository customerRepository,
        ISubscriptionRepository subscriptionRepository,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _clock = clock;
        _customerRepository = customerRepository;
        _subscriptionRepository = subscriptionRepository;
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateSubscriptionResult> HandleAsync(CreateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(command.CustomerId, cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException($"Customer '{command.CustomerId}' was not found.");
        }

        var subscription = Subscription.Create(
            command.CustomerId,
            command.PlanName,
            new Money(command.Amount, command.Currency),
            new BillingCycle(command.BillingInterval, command.BillingIntervalUnit),
            _clock.UtcNow);

        var initialInvoice = Invoice.Generate(subscription.GenerateInitialInvoice(_clock.UtcNow));

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _invoiceRepository.AddAsync(initialInvoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateSubscriptionResult(
            subscription.Id,
            subscription.CustomerId,
            subscription.PlanName,
            subscription.Status.ToString(),
            subscription.CurrentPeriodStartUtc,
            subscription.NextBillingDateUtc,
            initialInvoice.Id,
            subscription.Price.Amount,
            subscription.Price.Currency,
            subscription.BillingCycle.Interval,
            subscription.BillingCycle.Unit);
    }
}
