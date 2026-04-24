namespace SubscriptionBilling.Domain.Abstractions;

public abstract class ValueObject : IEquatable<ValueObject>
{
    public override bool Equals(object? obj)
    {
        return obj is ValueObject other && Equals(other);
    }

    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType())
        {
            return false;
        }

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(0, (hash, component) => HashCode.Combine(hash, component));
    }

    protected abstract IEnumerable<object?> GetEqualityComponents();
}
