using SubscriptionBilling.Application.Abstractions.Clock;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Application.Exceptions;

namespace SubscriptionBilling.Application.Features.Subscriptions;

public sealed class CancelSubscriptionCommandHandler : ICommandHandler<CancelSubscriptionCommand, CancelSubscriptionResult>
{
    private readonly IClock _clock;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelSubscriptionCommandHandler(
        IClock clock,
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork)
    {
        _clock = clock;
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CancelSubscriptionResult> HandleAsync(CancelSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(command.SubscriptionId, cancellationToken);

        if (subscription is null)
        {
            throw new NotFoundException($"Subscription '{command.SubscriptionId}' was not found.");
        }

        subscription.Cancel(_clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CancelSubscriptionResult(subscription.Id, subscription.Status.ToString(), subscription.CancelledOnUtc);
    }
}
