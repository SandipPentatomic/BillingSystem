namespace SubscriptionBilling.Infrastructure.Configuration;

public sealed class BillingProcessingOptions
{
    public const string SectionName = "BillingProcessing";

    public int BillingCyclePollingIntervalSeconds { get; init; } = 15;

    public int OutboxPollingIntervalSeconds { get; init; } = 10;
}
