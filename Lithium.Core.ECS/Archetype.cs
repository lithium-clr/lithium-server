using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

public sealed class Archetype(int capacity = 16)
{
    private Entity[] _entities = new Entity[capacity];
    public int Count { get; private set; }

    private Type[] _componentTypes = [];
    public ReadOnlySpan<Type> ComponentTypes => _componentTypes.AsSpan(0, _componentCount);
    private int _componentCount;

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
        var newTypes = new Type[_componentCount + 1];
        
        Array.Copy(_componentTypes, newTypes, _componentCount);
        newTypes[_componentCount] = type;
        SortTypesById(newTypes);
        
        return new ArchetypeKey(newTypes);
    }

    public ArchetypeKey GetKeyWithout(Type type)
    {
        var newTypes = new Type[_componentCount];
        var idx = 0;
        
        for (var i = 0; i < _componentCount; i++)
        {
            if (_componentTypes[i] != type)
                newTypes[idx++] = _componentTypes[i];
        }

        if (idx != newTypes.Length)
            Array.Resize(ref newTypes, idx);

        SortTypesById(newTypes);
        return new ArchetypeKey(newTypes);
    }

    public void AddComponentType(Type type)
    {
        if (_componentTypes.Length == _componentCount)
            Array.Resize(ref _componentTypes, Math.Max(4, _componentCount * 2));

        _componentTypes[_componentCount++] = type;
        SortTypesById(_componentTypes, _componentCount);
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SortTypesById(Type[] types, int length = -1)
    {
        if (length < 0) length = types.Length;
        
        Array.Sort(types, 0, length,
            Comparer<Type>.Create((a, b) => ComponentTypeId.GetId(a) - ComponentTypeId.GetId(b)));
    }

    public ref Entity this[int index] => ref _entities[index];
    public ReadOnlySpan<Entity> AsReadOnlySpan() => _entities.AsSpan(0, Count);
    public Span<Entity> AsSpan() => _entities.AsSpan(0, Count);

    public static readonly Archetype Empty = new(0);
}