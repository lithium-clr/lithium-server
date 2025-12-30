namespace Lithium.Core.ECS;

public sealed class Archetype(int capacity = 16)
{
    private Entity[] _entities = new Entity[capacity];
    public int Count { get; private set; }
    
    public HashSet<Type> ComponentTypes { get; } = [];

    public void Add(Entity entity)
    {
        if (Count == _entities.Length)
            Array.Resize(ref _entities, _entities.Length * 2);

        _entities[Count++] = entity;
    }

    public bool Remove(Entity entity)
    {
        for (var i = 0; i < Count; i++)
        {
            if (_entities[i].Id != entity.Id) continue;
            Count--;

            if (i != Count)
                _entities[i] = _entities[Count];

            _entities[Count] = default;
            return true;
        }

        return false;
    }
    
    public ArchetypeKey GetKeyWith(Type type)
    {
        var types = new List<Type>(ComponentTypes) { type };
        types.Sort((a, b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal));
        
        return new ArchetypeKey(types.ToArray());
    }

    public ArchetypeKey GetKeyWithout(Type type)
    {
        var types = new List<Type>(ComponentTypes);
        types.Remove(type);
        types.Sort((a, b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal));
        
        return new ArchetypeKey(types.ToArray());
    }

    public ref Entity this[int index] => ref _entities[index];
    public ReadOnlySpan<Entity> AsReadOnlySpan() => _entities.AsSpan(0, Count);
    public Span<Entity> AsSpan() => _entities.AsSpan(0, Count);

    public static readonly Archetype Empty = new(0);
}