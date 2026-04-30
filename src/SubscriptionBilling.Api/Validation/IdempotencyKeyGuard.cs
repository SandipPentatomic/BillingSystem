using Microsoft.AspNetCore.Http;

namespace SubscriptionBilling.Api.Validation;

public static class IdempotencyKeyGuard
{
    public static string Require(string? idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new BadHttpRequestException("The Idempotency-Key header is required for this operation.");
        }

        return idempotencyKey.Trim();
    }
}
