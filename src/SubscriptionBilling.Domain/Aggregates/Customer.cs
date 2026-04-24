using System.Diagnostics.CodeAnalysis;
using SubscriptionBilling.Domain.Abstractions;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Domain.Aggregates;

public sealed class Customer : AggregateRoot
{
    [ExcludeFromCodeCoverage]
    private Customer()
    {
    }

    private Customer(Guid id, string name, EmailAddress email, DateTime createdOnUtc)
    {
        Id = id;
        Name = name;
        Email = email;
        CreatedOnUtc = createdOnUtc;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public EmailAddress Email { get; private set; } = null!;

    public DateTime CreatedOnUtc { get; private set; }

    public static Customer Create(string name, string email, DateTime createdOnUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Customer name is required.");
        }

        return new Customer(
            Guid.NewGuid(),
            name.Trim(),
            new EmailAddress(email),
            createdOnUtc);
    }
}
