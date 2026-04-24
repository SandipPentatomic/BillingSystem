using SubscriptionBilling.Domain.Abstractions;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.ValueObjects;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SubscriptionBilling.Domain.Tests.ValueObjects;

public sealed class BillingCycleTests
{
    [Theory]
    [InlineData(1, BillingIntervalUnit.Minutes)]
    [InlineData(24, BillingIntervalUnit.Hours)]
    [InlineData(30, BillingIntervalUnit.Days)]
    [InlineData(12, BillingIntervalUnit.Months)]
    public void Creating_BillingCycle_With_Valid_Interval_And_Unit_Succeeds(int interval, BillingIntervalUnit unit)
    {
        var cycle = new BillingCycle(interval, unit);

        Assert.Equal(interval, cycle.Interval);
        Assert.Equal(unit, cycle.Unit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Creating_BillingCycle_With_Zero_Or_Negative_Interval_Throws_DomainException(int invalidInterval)
    {
        var exception = Assert.Throws<DomainException>(() => new BillingCycle(invalidInterval, BillingIntervalUnit.Months));

        Assert.Equal("Billing interval must be greater than zero.", exception.Message);
    }

    [Fact]
    public void Creating_BillingCycle_With_Invalid_Unit_Throws_DomainException()
    {
        var exception = Assert.Throws<DomainException>(() => new BillingCycle(1, (BillingIntervalUnit)999));

        Assert.Equal("Billing interval unit is invalid.", exception.Message);
    }

    [Theory]
    [InlineData(60 * 24 * 7 + 1, BillingIntervalUnit.Minutes)]
    [InlineData(24 * 31 + 1, BillingIntervalUnit.Hours)]
    [InlineData(366, BillingIntervalUnit.Days)]
    [InlineData(25, BillingIntervalUnit.Months)]
    public void Creating_BillingCycle_With_Interval_Too_Large_Throws_DomainException(int interval, BillingIntervalUnit unit)
    {
        var exception = Assert.Throws<DomainException>(() => new BillingCycle(interval, unit));

        Assert.Contains("Billing interval", exception.Message);
        Assert.Contains("is too large", exception.Message);
    }

    [Theory]
    [InlineData(60 * 24 * 7, BillingIntervalUnit.Minutes)]
    [InlineData(24 * 31, BillingIntervalUnit.Hours)]
    [InlineData(365, BillingIntervalUnit.Days)]
    [InlineData(24, BillingIntervalUnit.Months)]
    public void Creating_BillingCycle_With_Maximum_Valid_Interval_Succeeds(int interval, BillingIntervalUnit unit)
    {
        var cycle = new BillingCycle(interval, unit);

        Assert.Equal(interval, cycle.Interval);
    }

    [Fact]
    public void Adding_Minutes_BillingCycle_To_DateTime()
    {
        var cycle = new BillingCycle(30, BillingIntervalUnit.Minutes);
        var baseDate = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);

        var result = cycle.AddTo(baseDate);

        Assert.Equal(new DateTime(2026, 4, 1, 10, 30, 0, DateTimeKind.Utc), result);
    }

    [Fact]
    public void Adding_Hours_BillingCycle_To_DateTime()
    {
        var cycle = new BillingCycle(5, BillingIntervalUnit.Hours);
        var baseDate = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);

        var result = cycle.AddTo(baseDate);

        Assert.Equal(new DateTime(2026, 4, 1, 15, 0, 0, DateTimeKind.Utc), result);
    }

    [Fact]
    public void Adding_Days_BillingCycle_To_DateTime()
    {
        var cycle = new BillingCycle(7, BillingIntervalUnit.Days);
        var baseDate = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);

        var result = cycle.AddTo(baseDate);

        Assert.Equal(new DateTime(2026, 4, 8, 10, 0, 0, DateTimeKind.Utc), result);
    }

    [Fact]
    public void Adding_Months_BillingCycle_To_DateTime()
    {
        var cycle = new BillingCycle(1, BillingIntervalUnit.Months);
        var baseDate = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);

        var result = cycle.AddTo(baseDate);

        Assert.Equal(new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc), result);
    }

    [Fact]
    public void Adding_Multiple_Months_BillingCycle_To_DateTime()
    {
        var cycle = new BillingCycle(3, BillingIntervalUnit.Months);
        var baseDate = new DateTime(2026, 1, 31, 10, 0, 0, DateTimeKind.Utc);

        var result = cycle.AddTo(baseDate);

        Assert.Equal(new DateTime(2026, 4, 30, 10, 0, 0, DateTimeKind.Utc), result);
    }

    [Fact]
    public void Two_BillingCycle_Objects_With_Same_Values_Are_Equal()
    {
        var cycle1 = new BillingCycle(1, BillingIntervalUnit.Months);
        var cycle2 = new BillingCycle(1, BillingIntervalUnit.Months);

        Assert.Equal(cycle1, cycle2);
    }

    [Fact]
    public void Two_BillingCycle_Objects_With_Different_Intervals_Are_Not_Equal()
    {
        var cycle1 = new BillingCycle(1, BillingIntervalUnit.Months);
        var cycle2 = new BillingCycle(2, BillingIntervalUnit.Months);

        Assert.NotEqual(cycle1, cycle2);
    }

    [Fact]
    public void Two_BillingCycle_Objects_With_Different_Units_Are_Not_Equal()
    {
        var cycle1 = new BillingCycle(1, BillingIntervalUnit.Months);
        var cycle2 = new BillingCycle(1, BillingIntervalUnit.Days);

        Assert.NotEqual(cycle1, cycle2);
    }

    [Fact]
    public void Equal_BillingCycle_Objects_Have_Same_HashCode()
    {
        var cycle1 = new BillingCycle(1, BillingIntervalUnit.Months);
        var cycle2 = new BillingCycle(1, BillingIntervalUnit.Months);

        Assert.Equal(cycle1.GetHashCode(), cycle2.GetHashCode());
    }

    [Fact]
    public void AddTo_With_Invalid_Internal_Unit_Throws_DomainException()
    {
        var cycle = (BillingCycle)RuntimeHelpers.GetUninitializedObject(typeof(BillingCycle));
        typeof(BillingCycle).GetField("<Interval>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(cycle, 1);
        typeof(BillingCycle).GetField("<Unit>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(cycle, (BillingIntervalUnit)999);

        var exception = Assert.Throws<DomainException>(() => cycle.AddTo(DateTime.UtcNow));

        Assert.Equal("Billing interval unit is invalid.", exception.Message);
    }

    [Fact]
    public void ValidateRange_With_Invalid_Internal_Unit_Throws_DomainException()
    {
        var validateRange = typeof(BillingCycle).GetMethod("ValidateRange", BindingFlags.Static | BindingFlags.NonPublic)!;

        var exception = Assert.Throws<TargetInvocationException>(() => validateRange.Invoke(null, [1, (BillingIntervalUnit)999]));

        Assert.Equal("Billing interval '1' is too large for unit '999'.", Assert.IsType<DomainException>(exception.InnerException).Message);
    }
}
