using SubscriptionBilling.Application.Abstractions.Payments;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Infrastructure.Payments;
using SubscriptionBilling.Infrastructure.Tests.Support;

namespace SubscriptionBilling.Infrastructure.Tests.Payments;

public sealed class SimulatedPaymentGatewayTests
{
    [Theory]
    [InlineData(PaymentMode.Cash, "CASH")]
    [InlineData(PaymentMode.Check, "CHECK")]
    [InlineData(PaymentMode.Online, "ONLINE")]
    public async Task ChargeAsync_Returns_Contextual_Payment_Reference(PaymentMode paymentMode, string expectedPrefix)
    {
        var now = new DateTime(2026, 4, 28, 15, 0, 0, DateTimeKind.Utc);
        var gateway = new SimulatedPaymentGateway(new FakeClock(now));
        var invoiceId = Guid.NewGuid();

        var result = await gateway.ChargeAsync(
            new ChargePaymentRequest(invoiceId, Guid.NewGuid(), 59m, "USD", paymentMode),
            CancellationToken.None);

        Assert.StartsWith($"{expectedPrefix}-{invoiceId:N}-", result.PaymentReference);
        Assert.Equal(now, result.ProcessedOnUtc);
    }
}
