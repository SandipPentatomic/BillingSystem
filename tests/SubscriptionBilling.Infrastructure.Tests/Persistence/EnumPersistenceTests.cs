using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Infrastructure.Persistence;

namespace SubscriptionBilling.Infrastructure.Tests.Persistence;

public sealed class EnumPersistenceTests
{
    [Fact]
    public void ParseRequired_Returns_Enum_Value_For_Known_String()
    {
        var value = EnumPersistence.ParseRequired<InvoiceStatus>("Paid", "Invoice.Status");

        Assert.Equal(InvoiceStatus.Paid, value);
    }

    [Fact]
    public void ParseRequired_Throws_For_Unknown_String()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            EnumPersistence.ParseRequired<InvoiceStatus>("Retired", "Invoice.Status"));

        Assert.Contains("Persisted value 'Retired' is invalid", exception.Message);
    }

    [Fact]
    public void ParseNullable_Returns_Null_For_Empty_String()
    {
        var value = EnumPersistence.ParseNullable<PaymentMode>(null, "Invoice.PaymentMode");

        Assert.Null(value);
    }
}
