using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
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

        if (!TryNormalize(value, out var normalizedValue))
        {
            throw new DomainException("Email address is invalid.");
        }

        Value = normalizedValue;
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

    private static bool TryNormalize(string value, out string normalizedValue)
    {
        try
        {
            var mailAddress = new MailAddress(value);
            normalizedValue = mailAddress.Address.ToLowerInvariant();
            return string.Equals(mailAddress.Address, value, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            normalizedValue = string.Empty;
            return false;
        }
    }
}
