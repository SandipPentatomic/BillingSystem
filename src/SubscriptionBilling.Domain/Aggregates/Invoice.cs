using System.Diagnostics.CodeAnalysis;
using SubscriptionBilling.Domain.Abstractions;
using SubscriptionBilling.Domain.Billing;
using SubscriptionBilling.Domain.Enums;
using SubscriptionBilling.Domain.Events;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Domain.Aggregates;

public sealed class Invoice : AggregateRoot
{
    [ExcludeFromCodeCoverage]
    private Invoice()
    {
    }

    private Invoice(
        Guid id,
        Guid customerId,
        Guid subscriptionId,
        Money amount,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        DateTime dueDateUtc,
        DateTime issuedOnUtc)
    {
        Id = id;
        CustomerId = customerId;
        SubscriptionId = subscriptionId;
        Amount = amount;
        PeriodStartUtc = periodStartUtc;
        PeriodEndUtc = periodEndUtc;
        DueDateUtc = dueDateUtc;
        IssuedOnUtc = issuedOnUtc;
        Status = InvoiceStatus.Pending;
    }

    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public Guid SubscriptionId { get; private set; }

    public Money Amount { get; private set; } = null!;

    public InvoiceStatus Status { get; private set; }

    public DateTime PeriodStartUtc { get; private set; }

    public DateTime PeriodEndUtc { get; private set; }

    public DateTime DueDateUtc { get; private set; }

    public DateTime IssuedOnUtc { get; private set; }

    public DateTime? PaidOnUtc { get; private set; }

    public PaymentMode? PaymentMode { get; private set; }

    public static Invoice Generate(InvoiceGenerationDraft draft)
    {
        var invoice = new Invoice(
            draft.InvoiceId,
            draft.CustomerId,
            draft.SubscriptionId,
            draft.Amount,
            draft.PeriodStartUtc,
            draft.PeriodEndUtc,
            draft.DueDateUtc,
            draft.IssuedOnUtc);

        invoice.Raise(new InvoiceGeneratedDomainEvent(
            invoice.Id,
            invoice.SubscriptionId,
            invoice.CustomerId,
            invoice.Amount.Amount,
            invoice.Amount.Currency,
            draft.IssuedOnUtc));

        return invoice;
    }

    public void MarkAsPaid(DateTime paidOnUtc, PaymentMode paymentMode)
    {
        if (Status == InvoiceStatus.Paid)
        {
            throw new DomainException("Invoice cannot be paid twice.");
        }

        Status = InvoiceStatus.Paid;
        PaidOnUtc = paidOnUtc;
        PaymentMode = paymentMode;

        Raise(new PaymentReceivedDomainEvent(
            Id,
            SubscriptionId,
            CustomerId,
            Amount.Amount,
            Amount.Currency,
            paymentMode,
            paidOnUtc));
    }
}
