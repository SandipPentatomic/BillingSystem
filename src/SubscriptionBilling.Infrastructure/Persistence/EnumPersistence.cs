namespace SubscriptionBilling.Infrastructure.Persistence;

public static class EnumPersistence
{
    public static TEnum ParseRequired<TEnum>(string value, string propertyName)
        where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(value, true, out var parsedValue) && Enum.IsDefined(parsedValue))
        {
            return parsedValue;
        }

        throw new InvalidOperationException(
            $"Persisted value '{value}' is invalid for enum '{typeof(TEnum).Name}' on property '{propertyName}'.");
    }

    public static TEnum? ParseNullable<TEnum>(string? value, string propertyName)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return ParseRequired<TEnum>(value, propertyName);
    }
}
