using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using SubscriptionBilling.Domain.Abstractions;

namespace SubscriptionBilling.Infrastructure.Persistence;

public sealed class OutboxMessage
{
    [ExcludeFromCodeCoverage]
    private OutboxMessage()
    {
    }

    private OutboxMessage(Guid id, string type, string content, DateTime occurredOnUtc)
    {
        Id = id;
        Type = type;
        Content = content;
        OccurredOnUtc = occurredOnUtc;
    }

    public Guid Id { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public DateTime OccurredOnUtc { get; private set; }

    public DateTime? ProcessedOnUtc { get; private set; }

    public string? Error { get; private set; }

    public static OutboxMessage FromDomainEvent(IDomainEvent domainEvent)
    {
        return new OutboxMessage(
            Guid.NewGuid(),
            domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
            JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonDefaults.Options),
            domainEvent.OccurredOnUtc);
    }

    public void MarkProcessed(DateTime processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Error = error;
    }
}
