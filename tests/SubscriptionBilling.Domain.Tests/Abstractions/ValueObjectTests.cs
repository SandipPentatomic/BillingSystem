using SubscriptionBilling.Domain.Abstractions;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Domain.Tests.Abstractions;

public sealed class ValueObjectTests
{
    private sealed class FakeValueObject : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return "fake";
        }
    }

    [Fact]
    public void Two_Money_Objects_With_Same_Values_Are_Equal()
    {
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "USD");

        Assert.Equal(money1, money2);
    }

    [Fact]
    public void Two_Money_Objects_With_Different_Values_Are_Not_Equal()
    {
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "USD");

        Assert.NotEqual(money1, money2);
    }

    [Fact]
    public void Money_Object_Not_Equal_To_Null()
    {
        var money = new Money(100m, "USD");

        Assert.NotNull(money);
    }

    [Fact]
    public void Money_Object_Not_Equal_To_Different_Type()
    {
        var money = new Money(100m, "USD");

        Assert.NotEqual(money, (object)"100.00 USD");
    }

    [Fact]
    public void Equals_Returns_False_When_ValueObject_Is_Null()
    {
        var money = new Money(100m, "USD");

        var equals = money.Equals((ValueObject?)null);

        Assert.False(equals);
    }

    [Fact]
    public void Equals_Returns_False_When_ValueObject_Has_Different_Runtime_Type()
    {
        var money = new Money(100m, "USD");

        var equals = money.Equals(new FakeValueObject());

        Assert.False(equals);
    }

    [Fact]
    public void Equal_Money_Objects_Have_Same_HashCode()
    {
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "USD");

        Assert.Equal(money1.GetHashCode(), money2.GetHashCode());
    }

    [Fact]
    public void Two_EmailAddress_Objects_With_Same_Values_Are_Equal()
    {
        var email1 = new EmailAddress("test@example.com");
        var email2 = new EmailAddress("test@example.com");

        Assert.Equal(email1, email2);
    }

    [Fact]
    public void EmailAddress_Are_Case_Insensitive()
    {
        var email1 = new EmailAddress("Test@Example.com");
        var email2 = new EmailAddress("test@example.com");

        Assert.Equal(email1, email2);
    }

    [Fact]
    public void Two_BillingCycle_Objects_With_Same_Values_Are_Equal()
    {
        var cycle1 = new BillingCycle(1, BillingIntervalUnit.Months);
        var cycle2 = new BillingCycle(1, BillingIntervalUnit.Months);

        Assert.Equal(cycle1, cycle2);
    }

    [Fact]
    public void Two_BillingCycle_Objects_With_Different_Values_Are_Not_Equal()
    {
        var cycle1 = new BillingCycle(1, BillingIntervalUnit.Months);
        var cycle2 = new BillingCycle(2, BillingIntervalUnit.Months);

        Assert.NotEqual(cycle1, cycle2);
    }

    [Fact]
    public void Equal_BillingCycle_Objects_Have_Same_HashCode()
    {
        var cycle1 = new BillingCycle(1, BillingIntervalUnit.Months);
        var cycle2 = new BillingCycle(1, BillingIntervalUnit.Months);

        Assert.Equal(cycle1.GetHashCode(), cycle2.GetHashCode());
    }
}
