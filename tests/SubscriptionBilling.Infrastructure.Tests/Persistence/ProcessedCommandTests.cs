using SubscriptionBilling.Infrastructure.Persistence;

namespace SubscriptionBilling.Infrastructure.Tests.Persistence;

public sealed class ProcessedCommandTests
{
    [Fact]
    public void Create_Populates_All_Properties()
    {
        var createdOn = new DateTime(2026, 4, 24, 10, 0, 0, DateTimeKind.Utc);

        var command = ProcessedCommand.Create("command-key", "{\"value\":\"a\"}", createdOn);

        Assert.Equal("command-key", command.IdempotencyKey);
        Assert.Equal("{\"value\":\"a\"}", command.ResponseJson);
        Assert.Equal(createdOn, command.CreatedOnUtc);
    }

    [Fact]
    public void UpdateResponse_Replaces_Response_Json()
    {
        var command = ProcessedCommand.Create("command-key", "{\"value\":\"a\"}", DateTime.UtcNow);

        command.UpdateResponse("{\"value\":\"b\"}");

        Assert.Equal("{\"value\":\"b\"}", command.ResponseJson);
    }
}
