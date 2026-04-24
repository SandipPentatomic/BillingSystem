using SubscriptionBilling.Domain.Events;
using SubscriptionBilling.Infrastructure.Persistence;

namespace SubscriptionBilling.Infrastructure.Tests.Persistence;

public sealed class OutboxMessageTests
{
    [Fact]
    public void FromDomainEvent_Creates_Persistable_Message()
    {
        var occurredOn = new DateTime(2026, 4, 24, 9, 30, 0, DateTimeKind.Utc);
        var domainEvent = new PaymentReceivedDomainEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            59m,
            "USD",
            SubscriptionBilling.Domain.Enums.PaymentMode.Online,
            occurredOn);

        var message = OutboxMessage.FromDomainEvent(domainEvent);

        Assert.NotEqual(Guid.Empty, message.Id);
        Assert.Contains(nameof(PaymentReceivedDomainEvent), message.Type);
        Assert.Contains("\"amount\":59", message.Content, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(occurredOn, message.OccurredOnUtc);
    }

    [Fact]
    public void MarkProcessed_Sets_Processed_Time_And_Clears_Error()
    {
        var message = OutboxMessage.FromDomainEvent(new SubscriptionBilling.Domain.Events.InvoiceGeneratedDomainEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            10m,
            "USD",
            DateTime.UtcNow));

        message.MarkFailed("boom");
        message.MarkProcessed(new DateTime(2026, 4, 24, 9, 45, 0, DateTimeKind.Utc));

        Assert.NotNull(message.ProcessedOnUtc);
        Assert.Null(message.Error);
    }

    [Fact]
    public void MarkFailed_Sets_Error_Message()
    {
        var message = OutboxMessage.FromDomainEvent(new SubscriptionBilling.Domain.Events.SubscriptionActivatedDomainEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow));

        message.MarkFailed("processing failed");

        Assert.Equal("processing failed", message.Error);
    }
}
