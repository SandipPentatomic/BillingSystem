using SubscriptionBilling.Domain.Abstractions;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Domain.Tests.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Create_Money_With_Valid_Amount_And_Currency()
    {
        var money = new Money(100m, "USD");

        Assert.Equal(100m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Theory]
    [InlineData(0.01, "USD")]
    [InlineData(1, "EUR")]
    [InlineData(999999.99, "GBP")]
    public void Create_Money_With_Valid_Amounts(decimal amount, string currency)
    {
        var money = new Money(amount, currency);

        Assert.Equal(amount, money.Amount);
        Assert.Equal(currency.ToUpperInvariant(), money.Currency);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(-0.01)]
    public void Creating_Money_With_Zero_Or_Negative_Amount_Throws_DomainException(decimal invalidAmount)
    {
        var exception = Assert.Throws<DomainException>(() => new Money(invalidAmount, "USD"));

        Assert.Equal("Money amount must be greater than zero.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Creating_Money_With_Empty_Currency_Throws_DomainException(string? invalidCurrency)
    {
        var exception = Assert.Throws<DomainException>(() => new Money(100m, invalidCurrency!));

        Assert.Equal("Currency is required.", exception.Message);
    }

    [Theory]
    [InlineData("U")]
    [InlineData("US")]
    [InlineData("USDA")]
    public void Creating_Money_With_Invalid_Currency_Length_Throws_DomainException(string invalidCurrency)
    {
        var exception = Assert.Throws<DomainException>(() => new Money(100m, invalidCurrency));

        Assert.Equal("Currency must be a 3-letter ISO code.", exception.Message);
    }

    [Fact]
    public void Money_Rounds_Amount_To_Two_Decimal_Places()
    {
        var money = new Money(100.123m, "USD");

        Assert.Equal(100.12m, money.Amount);
    }

    [Fact]
    public void Money_Rounds_Away_From_Zero()
    {
        var money = new Money(100.125m, "USD");

        Assert.Equal(100.13m, money.Amount);
    }

    [Fact]
    public void Currency_Is_Converted_To_Uppercase()
    {
        var money = new Money(100m, "usd");

        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Currency_Is_Trimmed()
    {
        var money = new Money(100m, "  USD  ");

        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void To_String_Returns_Formatted_Amount_And_Currency()
    {
        var money = new Money(100.5m, "USD");

        Assert.Equal("100.50 USD", money.ToString());
    }

    [Fact]
    public void Two_Money_Objects_With_Same_Values_Are_Equal()
    {
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "usd");

        Assert.Equal(money1, money2);
    }

    [Fact]
    public void Two_Money_Objects_With_Different_Amounts_Are_Not_Equal()
    {
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "USD");

        Assert.NotEqual(money1, money2);
    }

    [Fact]
    public void Two_Money_Objects_With_Different_Currencies_Are_Not_Equal()
    {
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "EUR");

        Assert.NotEqual(money1, money2);
    }

    [Fact]
    public void Equal_Money_Objects_Have_Same_HashCode()
    {
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "USD");

        Assert.Equal(money1.GetHashCode(), money2.GetHashCode());
    }
}
