using Microsoft.EntityFrameworkCore;
using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Application.ReadModels;
using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Infrastructure.Persistence.Repositories;

public sealed class InvoiceReadRepository : IInvoiceReadRepository
{
    private readonly BillingDbContext _dbContext;

    public InvoiceReadRepository(BillingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<InvoiceListItem>> ListAsync(
        Guid? customerId,
        Guid? subscriptionId,
        InvoiceStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (pageNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
        }

        if (pageSize <= 0 || pageSize > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 200.");
        }

        var query = _dbContext.Invoices.AsNoTracking().AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(invoice => invoice.CustomerId == customerId.Value);
        }

        if (subscriptionId.HasValue)
        {
            query = query.Where(invoice => invoice.SubscriptionId == subscriptionId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(invoice => invoice.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(invoice => invoice.IssuedOnUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
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
                invoice.PaymentMode,
                invoice.ExternalPaymentReference))
            .ToListAsync(cancellationToken);

        return new PagedResult<InvoiceListItem>(items, pageNumber, pageSize, totalCount);
    }
}
