namespace FormBuilder.Domain.Common;

/// <summary>
/// Base class for domain entities. Identity is carried by <see cref="Id"/>: two entities of
/// the same runtime type with the same id are considered equal, regardless of their state.
/// </summary>
public abstract class Entity
{
    /// <summary>Stable identity of the entity, generated when the entity is first created.</summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Entity other
            && other.GetType() == GetType()
            && other.Id == Id;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !Equals(left, right);
    }
}
