using Microsoft.EntityFrameworkCore;
using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Application.Features.Invoices;
using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Infrastructure.Persistence.Repositories;

public sealed class InvoiceReadRepository : IInvoiceReadRepository
{
    private readonly BillingDbContext _dbContext;

    public InvoiceReadRepository(BillingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<InvoiceListItem>> ListAsync(
        Guid? customerId,
        Guid? subscriptionId,
        string? status,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Invoices.AsNoTracking().AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(invoice => invoice.CustomerId == customerId.Value);
        }

        if (subscriptionId.HasValue)
        {
            query = query.Where(invoice => invoice.SubscriptionId == subscriptionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<InvoiceStatus>(status, true, out var parsedStatus))
            {
                return [];
            }

            query = query.Where(invoice => invoice.Status == parsedStatus);
        }

        return await query
            .OrderByDescending(invoice => invoice.IssuedOnUtc)
            .Select(invoice => new InvoiceListItem(
                invoice.Id,
                invoice.CustomerId,
                invoice.SubscriptionId,
                invoice.Amount.Amount,
                invoice.Amount.Currency,
                invoice.Status.ToString(),
                invoice.PeriodStartUtc,
                invoice.PeriodEndUtc,
                invoice.DueDateUtc,
                invoice.IssuedOnUtc,
                invoice.PaidOnUtc,
                invoice.PaymentMode))
            .ToListAsync(cancellationToken);
    }
}
