using System.Diagnostics.CodeAnalysis;

namespace SubscriptionBilling.Infrastructure.Persistence;

public sealed class ProcessedCommand
{
    [ExcludeFromCodeCoverage]
    private ProcessedCommand()
    {
    }

    private ProcessedCommand(string idempotencyKey, string responseJson, DateTime createdOnUtc)
    {
        IdempotencyKey = idempotencyKey;
        ResponseJson = responseJson;
        CreatedOnUtc = createdOnUtc;
    }

    public string IdempotencyKey { get; private set; } = string.Empty;

    public string ResponseJson { get; private set; } = string.Empty;

    public DateTime CreatedOnUtc { get; private set; }

    public static ProcessedCommand Create(string idempotencyKey, string responseJson, DateTime createdOnUtc)
    {
        return new ProcessedCommand(idempotencyKey, responseJson, createdOnUtc);
    }

    public void UpdateResponse(string responseJson)
    {
        ResponseJson = responseJson;
    }
}
