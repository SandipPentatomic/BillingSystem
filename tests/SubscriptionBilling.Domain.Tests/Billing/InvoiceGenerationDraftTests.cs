using SubscriptionBilling.Domain.Billing;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Domain.Tests.Billing;

public sealed class InvoiceGenerationDraftTests
{
    [Fact]
    public void Creating_InvoiceGenerationDraft_With_All_Values_Succeeds()
    {
        var invoiceId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var amount = new Money(100m, "USD");
        var periodStart = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc);
        var dueDate = new DateTime(2026, 5, 7, 0, 0, 0, DateTimeKind.Utc);
        var issuedOn = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        var draft = new InvoiceGenerationDraft(
            invoiceId,
            customerId,
            subscriptionId,
            amount,
            periodStart,
            periodEnd,
            dueDate,
            issuedOn);

        Assert.Equal(invoiceId, draft.InvoiceId);
        Assert.Equal(customerId, draft.CustomerId);
        Assert.Equal(subscriptionId, draft.SubscriptionId);
        Assert.Equal(amount, draft.Amount);
        Assert.Equal(periodStart, draft.PeriodStartUtc);
        Assert.Equal(periodEnd, draft.PeriodEndUtc);
        Assert.Equal(dueDate, draft.DueDateUtc);
        Assert.Equal(issuedOn, draft.IssuedOnUtc);
    }

    [Fact]
    public void Two_InvoiceGenerationDraft_Records_With_Same_Values_Are_Equal()
    {
        var invoiceId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var amount = new Money(100m, "USD");
        var periodStart = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc);
        var dueDate = new DateTime(2026, 5, 7, 0, 0, 0, DateTimeKind.Utc);
        var issuedOn = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        var draft1 = new InvoiceGenerationDraft(
            invoiceId,
            customerId,
            subscriptionId,
            amount,
            periodStart,
            periodEnd,
            dueDate,
            issuedOn);

        var draft2 = new InvoiceGenerationDraft(
            invoiceId,
            customerId,
            subscriptionId,
            amount,
            periodStart,
            periodEnd,
            dueDate,
            issuedOn);

        Assert.Equal(draft1, draft2);
    }

    [Fact]
    public void Two_InvoiceGenerationDraft_Records_With_Different_InvoiceIds_Are_Not_Equal()
    {
        var customerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var amount = new Money(100m, "USD");
        var periodStart = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc);
        var dueDate = new DateTime(2026, 5, 7, 0, 0, 0, DateTimeKind.Utc);
        var issuedOn = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        var draft1 = new InvoiceGenerationDraft(
            Guid.NewGuid(),
            customerId,
            subscriptionId,
            amount,
            periodStart,
            periodEnd,
            dueDate,
            issuedOn);

        var draft2 = new InvoiceGenerationDraft(
            Guid.NewGuid(),
            customerId,
            subscriptionId,
            amount,
            periodStart,
            periodEnd,
            dueDate,
            issuedOn);

        Assert.NotEqual(draft1, draft2);
    }

    [Fact]
    public void InvoiceGenerationDraft_Is_Record_Type()
    {
        var invoiceId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var amount = new Money(100m, "USD");
        var periodStart = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc);
        var dueDate = new DateTime(2026, 5, 7, 0, 0, 0, DateTimeKind.Utc);
        var issuedOn = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        var draft = new InvoiceGenerationDraft(
            invoiceId,
            customerId,
            subscriptionId,
            amount,
            periodStart,
            periodEnd,
            dueDate,
            issuedOn);

        var draftString = draft.ToString();

        Assert.NotNull(draftString);
        Assert.Contains(nameof(InvoiceGenerationDraft), draftString);
    }
}
