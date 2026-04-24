using SubscriptionBilling.Infrastructure.Services;

namespace SubscriptionBilling.Infrastructure.Tests.Services;

public sealed class SystemClockTests
{
    [Fact]
    public void UtcNow_Returns_Current_Utc_Time()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var clock = new SystemClock();

        var now = clock.UtcNow;

        var after = DateTime.UtcNow.AddSeconds(1);
        Assert.InRange(now, before, after);
    }
}
