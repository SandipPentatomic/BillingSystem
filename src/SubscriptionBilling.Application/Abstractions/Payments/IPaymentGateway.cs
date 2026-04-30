namespace SubscriptionBilling.Application.Abstractions.Payments;

public interface IPaymentGateway
{
    Task<ChargePaymentResult> ChargeAsync(ChargePaymentRequest request, CancellationToken cancellationToken);
}
