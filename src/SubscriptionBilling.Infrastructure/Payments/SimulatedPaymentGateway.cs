using SubscriptionBilling.Application.Abstractions.Clock;
using SubscriptionBilling.Application.Abstractions.Payments;
using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Infrastructure.Payments;

public sealed class SimulatedPaymentGateway : IPaymentGateway
{
    private readonly IClock _clock;

    public SimulatedPaymentGateway(IClock clock)
    {
        _clock = clock;
    }

    public Task<ChargePaymentResult> ChargeAsync(ChargePaymentRequest request, CancellationToken cancellationToken)
    {
        var prefix = request.PaymentMode switch
        {
            PaymentMode.Cash => "CASH",
            PaymentMode.Check => "CHECK",
            PaymentMode.Online => "ONLINE",
            _ => "PAYMENT"
        };

        var paymentReference = $"{prefix}-{request.InvoiceId:N}-{_clock.UtcNow:yyyyMMddHHmmss}";

        return Task.FromResult(new ChargePaymentResult(paymentReference, _clock.UtcNow));
    }
}
