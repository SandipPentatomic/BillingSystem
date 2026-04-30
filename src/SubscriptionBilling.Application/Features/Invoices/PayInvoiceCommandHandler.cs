using SubscriptionBilling.Application.Abstractions.Clock;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Abstractions.Payments;
using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Application.Exceptions;

namespace SubscriptionBilling.Application.Features.Invoices;

public sealed class PayInvoiceCommandHandler : ICommandHandler<PayInvoiceCommand, PayInvoiceResult>
{
    private readonly IClock _clock;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IUnitOfWork _unitOfWork;

    public PayInvoiceCommandHandler(
        IClock clock,
        IInvoiceRepository invoiceRepository,
        IPaymentGateway paymentGateway,
        IUnitOfWork unitOfWork)
    {
        _clock = clock;
        _invoiceRepository = invoiceRepository;
        _paymentGateway = paymentGateway;
        _unitOfWork = unitOfWork;
    }

    public async Task<PayInvoiceResult> HandleAsync(PayInvoiceCommand command, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken);

        if (invoice is null)
        {
            throw new NotFoundException($"Invoice '{command.InvoiceId}' was not found.");
        }

        var paymentResult = await _paymentGateway.ChargeAsync(
            new ChargePaymentRequest(
                invoice.Id,
                invoice.CustomerId,
                invoice.Amount.Amount,
                invoice.Amount.Currency,
                command.PaymentMode),
            cancellationToken);

        invoice.MarkAsPaid(paymentResult.ProcessedOnUtc, command.PaymentMode, paymentResult.PaymentReference);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PayInvoiceResult(
            invoice.Id,
            invoice.Status.ToString(),
            invoice.PaidOnUtc!.Value,
            invoice.PaymentMode!.Value,
            invoice.ExternalPaymentReference!);
    }
}
