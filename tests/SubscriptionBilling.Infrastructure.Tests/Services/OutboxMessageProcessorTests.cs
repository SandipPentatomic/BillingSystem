using Microsoft.Extensions.Logging.Abstractions;
using SubscriptionBilling.Domain.Events;
using SubscriptionBilling.Infrastructure.Persistence;
using SubscriptionBilling.Infrastructure.Services;
using SubscriptionBilling.Infrastructure.Tests.Support;

namespace SubscriptionBilling.Infrastructure.Tests.Services;

public sealed class OutboxMessageProcessorTests
{
    [Fact]
    public async Task ProcessPendingAsync_Marks_Pending_Messages_As_Processed()
    {
        var now = new DateTime(2026, 4, 28, 10, 0, 0, DateTimeKind.Utc);
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.OutboxMessages.Add(OutboxMessage.FromDomainEvent(new SubscriptionActivatedDomainEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            now.AddMinutes(-10))));
        await dbContext.SaveChangesAsync();

        var processor = new OutboxMessageProcessor(dbContext, new FakeClock(now), NullLogger<OutboxMessageProcessor>.Instance);

        await processor.ProcessPendingAsync(CancellationToken.None);

        var message = Assert.Single(dbContext.OutboxMessages);
        Assert.Equal(now, message.ProcessedOnUtc);
        Assert.Null(message.Error);
    }

    [Fact]
    public async Task ProcessPendingAsync_Does_Nothing_When_No_Pending_Messages_Exist()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var processor = new OutboxMessageProcessor(dbContext, new FakeClock(DateTime.UtcNow), NullLogger<OutboxMessageProcessor>.Instance);

        await processor.ProcessPendingAsync(CancellationToken.None);

        Assert.Empty(dbContext.OutboxMessages);
    }
}
