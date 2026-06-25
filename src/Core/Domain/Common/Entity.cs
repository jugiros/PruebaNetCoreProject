namespace PruebaNetCoreProject.Domain.Common;

public abstract class Entity<TId>
    where TId : struct, IEquatable<TId>
{
    public TId Id { get; protected init; }

    public override bool Equals(object? obj)
    {
        if (obj is null || obj is not Entity<TId> other)
            return false;

        if (GetType() != other.GetType())
            return false;

        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return (GetType().ToString() + Id).GetHashCode(StringComparison.Ordinal);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }
}
