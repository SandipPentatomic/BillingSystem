using SubscriptionBilling.Infrastructure.Configuration;

namespace SubscriptionBilling.Infrastructure.Tests.Configuration;

public sealed class BillingProcessingOptionsTests
{
    [Fact]
    public void Options_Have_Default_Intervals_And_Section_Name()
    {
        var options = new BillingProcessingOptions();

        Assert.Equal("BillingProcessing", BillingProcessingOptions.SectionName);
        Assert.Equal(15, options.BillingCyclePollingIntervalSeconds);
        Assert.Equal(10, options.OutboxPollingIntervalSeconds);
    }
}
