using SubscriptionBilling.Infrastructure.Persistence;
using SubscriptionBilling.Infrastructure.Tests.Support;

namespace SubscriptionBilling.Infrastructure.Tests.Persistence;

public sealed class IdempotencyStoreTests
{
    [Fact]
    public async Task GetResponseAsync_Returns_Null_When_Key_Does_Not_Exist()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var store = new IdempotencyStore(dbContext, new FakeClock(DateTime.UtcNow));

        var response = await store.GetResponseAsync("missing-key", CancellationToken.None);

        Assert.Null(response);
    }

    [Fact]
    public async Task SaveResponseAsync_Creates_New_Processed_Command_When_Key_Does_Not_Exist()
    {
        var now = new DateTime(2026, 4, 24, 9, 0, 0, DateTimeKind.Utc);
        await using var dbContext = TestDbContextFactory.Create();
        var store = new IdempotencyStore(dbContext, new FakeClock(now));

        await store.SaveResponseAsync("new-key", "{\"value\":\"first\"}", CancellationToken.None);

        var processedCommand = Assert.Single(dbContext.ProcessedCommands);
        Assert.Equal("new-key", processedCommand.IdempotencyKey);
        Assert.Equal("{\"value\":\"first\"}", processedCommand.ResponseJson);
        Assert.Equal(now, processedCommand.CreatedOnUtc);
    }

    [Fact]
    public async Task SaveResponseAsync_Updates_Existing_Response_When_Key_Already_Exists()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.ProcessedCommands.Add(ProcessedCommand.Create("existing-key", "{\"value\":\"old\"}", DateTime.UtcNow.AddMinutes(-5)));
        await dbContext.SaveChangesAsync();

        var store = new IdempotencyStore(dbContext, new FakeClock(DateTime.UtcNow));

        await store.SaveResponseAsync("existing-key", "{\"value\":\"new\"}", CancellationToken.None);

        var processedCommand = Assert.Single(dbContext.ProcessedCommands);
        Assert.Equal("{\"value\":\"new\"}", processedCommand.ResponseJson);
    }
}
