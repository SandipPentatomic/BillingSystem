using SubscriptionBilling.Application.Abstractions.Clock;

namespace SubscriptionBilling.Infrastructure.Services;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
