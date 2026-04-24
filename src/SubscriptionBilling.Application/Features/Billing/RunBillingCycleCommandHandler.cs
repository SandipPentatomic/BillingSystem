using SubscriptionBilling.Application.Abstractions.Clock;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Domain.Aggregates;

namespace SubscriptionBilling.Application.Features.Billing;

public sealed class RunBillingCycleCommandHandler : ICommandHandler<RunBillingCycleCommand, RunBillingCycleResult>
{
    private readonly IClock _clock;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RunBillingCycleCommandHandler(
        IClock clock,
        ISubscriptionRepository subscriptionRepository,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _clock = clock;
        _subscriptionRepository = subscriptionRepository;
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RunBillingCycleResult> HandleAsync(RunBillingCycleCommand command, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var dueSubscriptions = await _subscriptionRepository.ListDueForBillingAsync(now, cancellationToken);

        var invoiceCount = 0;

        foreach (var subscription in dueSubscriptions)
        {
            var invoices = subscription.GenerateDueInvoices(now)
                .Select(Invoice.Generate)
                .ToArray();

            if (invoices.Length == 0)
            {
                continue;
            }

            await _invoiceRepository.AddRangeAsync(invoices, cancellationToken);
            invoiceCount += invoices.Length;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RunBillingCycleResult(dueSubscriptions.Count, invoiceCount, now);
    }
}
