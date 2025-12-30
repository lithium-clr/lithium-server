namespace Lithium.Core.ECS;

public sealed class Archetype(int capacity = 16)
{
    private Entity[] _entities = new Entity[capacity];
    public int Count { get; private set; }

    public void Add(Entity entity)
    {
        if (Count == _entities.Length)
            Array.Resize(ref _entities, _entities.Length * 2);
    
        _entities[Count++] = entity;
    }

    public ref Entity this[int index] => ref _entities[index];
    public ReadOnlySpan<Entity> AsReadOnlySpan() => _entities.AsSpan(0, Count);
    public Span<Entity> AsSpan() => _entities.AsSpan(0, Count);
    
    public static readonly Archetype Empty = new(0);
}