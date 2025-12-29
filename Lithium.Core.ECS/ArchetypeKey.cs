namespace Lithium.Core.ECS;

public readonly struct ArchetypeKey : IEquatable<ArchetypeKey>
{
    private readonly int _hash;

    public ArchetypeKey(ReadOnlySpan<Type> types)
    {
        unchecked
        {
            var hash = 17;

            foreach (var t in types)
                hash *= 31 + t.GetHashCode();

            _hash = hash;
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is ArchetypeKey key && Equals(key);
    }

    public bool Equals(ArchetypeKey other) => _hash == other._hash;

    public override int GetHashCode() => _hash;

    public static bool operator ==(ArchetypeKey left, ArchetypeKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ArchetypeKey left, ArchetypeKey right)
    {
        return !(left == right);
    }
}