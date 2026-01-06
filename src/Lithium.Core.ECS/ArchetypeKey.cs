namespace Lithium.Core.ECS;

public readonly struct ArchetypeKey : IEquatable<ArchetypeKey>
{
    private readonly int _hash;
    private readonly int[] _typeIds;
    
    public static readonly ArchetypeKey Empty = new(Array.Empty<Type>());

    public ArchetypeKey(ReadOnlySpan<Type> types)
    {
        unchecked
        {
            var hash = 17;
            _typeIds = new int[types.Length];

            for (int i = 0; i < types.Length; i++)
            {
                var t = types[i];
                int typeId;
                
                if (typeof(IComponent).IsAssignableFrom(t))
                    typeId = ComponentTypeId.GetId(t);
                else if (typeof(ITag).IsAssignableFrom(t))
                    typeId = TagTypeId.GetId(t);
                else
                    throw new ArgumentException($"Type {t.FullName} is not a component or tag");

                _typeIds[i] = typeId;
                hash = hash * 31 + typeId;
            }

            _hash = hash;
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is ArchetypeKey key && Equals(key);
    }

    public bool Equals(ArchetypeKey other)
    {
        if (_hash != other._hash) return false;
        if (_typeIds.Length != other._typeIds.Length) return false;

        for (int i = 0; i < _typeIds.Length; i++)
        {
            if (_typeIds[i] != other._typeIds[i]) return false;
        }

        return true;
    }

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