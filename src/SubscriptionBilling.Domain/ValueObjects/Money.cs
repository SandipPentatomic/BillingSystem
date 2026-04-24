using System.Diagnostics.CodeAnalysis;
using SubscriptionBilling.Domain.Abstractions;

namespace SubscriptionBilling.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    [ExcludeFromCodeCoverage]
    private Money()
    {
    }

    public Money(decimal amount, string currency)
    {
        if (amount <= 0)
        {
            throw new DomainException("Money amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("Currency is required.");
        }

        currency = currency.Trim().ToUpperInvariant();

        if (currency.Length != 3)
        {
            throw new DomainException("Currency must be a 3-letter ISO code.");
        }

        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency;
    }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString()
    {
        return $"{Amount:0.00} {Currency}";
    }
}
