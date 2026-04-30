using SubscriptionBilling.Domain.Abstractions;
using SubscriptionBilling.Domain.Events;

namespace SubscriptionBilling.Infrastructure.Persistence;

public static class OutboxEventTypeRegistry
{
    private static readonly IReadOnlyDictionary<Type, string> TypeToDiscriminator = new Dictionary<Type, string>
    {
        [typeof(SubscriptionActivatedDomainEvent)] = "subscription-activated",
        [typeof(InvoiceGeneratedDomainEvent)] = "invoice-generated",
        [typeof(PaymentReceivedDomainEvent)] = "payment-received"
    };

    private static readonly IReadOnlyDictionary<string, Type> DiscriminatorToType = TypeToDiscriminator
        .ToDictionary(pair => pair.Value, pair => pair.Key, StringComparer.OrdinalIgnoreCase);

    public static string GetDiscriminator(IDomainEvent domainEvent)
    {
        return GetDiscriminator(domainEvent.GetType());
    }

    public static string GetDiscriminator(Type domainEventType)
    {
        if (TypeToDiscriminator.TryGetValue(domainEventType, out var discriminator))
        {
            return discriminator;
        }

        throw new InvalidOperationException($"No outbox mapping is registered for domain event '{domainEventType.Name}'.");
    }

    public static string GetDisplayName(string discriminator)
    {
        if (DiscriminatorToType.TryGetValue(discriminator, out var domainEventType))
        {
            return domainEventType.Name;
        }

        return discriminator;
    }
}
