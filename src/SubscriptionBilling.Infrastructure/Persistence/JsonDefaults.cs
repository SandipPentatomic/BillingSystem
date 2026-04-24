using System.Text.Json;

namespace SubscriptionBilling.Infrastructure.Persistence;

internal static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
