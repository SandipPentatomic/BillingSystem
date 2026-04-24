using SubscriptionBilling.Application.Abstractions.Clock;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Application.Exceptions;

namespace SubscriptionBilling.Application.Features.Invoices;

public sealed class PayInvoiceCommandHandler : ICommandHandler<PayInvoiceCommand, PayInvoiceResult>
{
    private readonly IClock _clock;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PayInvoiceCommandHandler(
        IClock clock,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _clock = clock;
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PayInvoiceResult> HandleAsync(PayInvoiceCommand command, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken);

        if (invoice is null)
        {
            throw new NotFoundException($"Invoice '{command.InvoiceId}' was not found.");
        }

        invoice.MarkAsPaid(_clock.UtcNow, command.PaymentMode);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PayInvoiceResult(invoice.Id, invoice.Status.ToString(), invoice.PaidOnUtc!.Value, invoice.PaymentMode!.Value);
    }
}
