namespace SubscriptionBilling.Api.Extensions;

public static class HttpContextExtensions
{
    public static string GetIdempotencyKey(this HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("Idempotency-Key", out var value) &&
            !string.IsNullOrWhiteSpace(value))
        {
            return value.ToString();
        }

        return Guid.NewGuid().ToString("N");
    }
}
