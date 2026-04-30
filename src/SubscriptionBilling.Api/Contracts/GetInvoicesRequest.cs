using System.ComponentModel.DataAnnotations;
using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Api.Contracts;

public sealed record GetInvoicesRequest(
    Guid? CustomerId,
    Guid? SubscriptionId,
    InvoiceStatus? Status,
    [Range(1, int.MaxValue)] int PageNumber = 1,
    [Range(1, 200)] int PageSize = 50);
