using System.Diagnostics.CodeAnalysis;
using SubscriptionBilling.Domain.Abstractions;

namespace SubscriptionBilling.Domain.ValueObjects;

public sealed class EmailAddress : ValueObject
{
    [ExcludeFromCodeCoverage]
    private EmailAddress()
    {
    }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Email address is required.");
        }

        value = value.Trim();

        if (!value.Contains('@') || value.StartsWith('@') || value.EndsWith('@'))
        {
            throw new DomainException("Email address is invalid.");
        }

        Value = value.ToLowerInvariant();
    }

    public string Value { get; private set; } = string.Empty;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
