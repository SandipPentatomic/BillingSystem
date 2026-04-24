using System.Diagnostics.CodeAnalysis;
using SubscriptionBilling.Domain.Abstractions;
using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Domain.ValueObjects;

public sealed class BillingCycle : ValueObject
{
    [ExcludeFromCodeCoverage]
    private BillingCycle()
    {
    }

    public BillingCycle(int interval, BillingIntervalUnit unit)
    {
        if (interval <= 0)
        {
            throw new DomainException("Billing interval must be greater than zero.");
        }

        if (!Enum.IsDefined(unit))
        {
            throw new DomainException("Billing interval unit is invalid.");
        }

        ValidateRange(interval, unit);

        Interval = interval;
        Unit = unit;
    }

    public int Interval { get; private set; }

    public BillingIntervalUnit Unit { get; private set; }

    public DateTime AddTo(DateTime dateTimeUtc)
    {
        return Unit switch
        {
            BillingIntervalUnit.Minutes => dateTimeUtc.AddMinutes(Interval),
            BillingIntervalUnit.Hours => dateTimeUtc.AddHours(Interval),
            BillingIntervalUnit.Days => dateTimeUtc.AddDays(Interval),
            BillingIntervalUnit.Months => dateTimeUtc.AddMonths(Interval),
            _ => throw new DomainException("Billing interval unit is invalid.")
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Interval;
        yield return Unit;
    }

    private static void ValidateRange(int interval, BillingIntervalUnit unit)
    {
        var isValid = unit switch
        {
            BillingIntervalUnit.Minutes => interval <= 60 * 24 * 7,
            BillingIntervalUnit.Hours => interval <= 24 * 31,
            BillingIntervalUnit.Days => interval <= 365,
            BillingIntervalUnit.Months => interval <= 24,
            _ => false
        };

        if (!isValid)
        {
            throw new DomainException($"Billing interval '{interval}' is too large for unit '{unit}'.");
        }
    }
}
