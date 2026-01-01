using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

public interface ISparseSet
{
    int Count { get; }
    ReadOnlySpan<EntityId> Entities { get; }

    IComponent GetComponent(Entity entity);
    bool Has(Entity entity);
}

public sealed class SparseSet<T> : ISparseSet where T : struct
{
    private T[] _dense = new T[16];
    private EntityId[] _entities = new EntityId[16];
    private readonly Dictionary<EntityId, int> _sparse = new();

    public int Count { get; private set; }
    public ReadOnlySpan<EntityId> Entities => _entities.AsSpan(0, Count);

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Entity entity, T component)
    {
        if (_sparse.TryGetValue(entity.Id, out var idx))
        {
            _dense[idx] = component;
            return;
        }

        if (Count == _dense.Length)
            Grow();

        _dense[Count] = component;
        _entities[Count] = entity.Id;
        _sparse[entity.Id] = Count;
        Count++;
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Entity entity)
    {
        if (!_sparse.TryGetValue(entity.Id, out var idx))
            return;

        var last = --Count;
        _dense[idx] = _dense[last];
        _entities[idx] = _entities[last];
        _sparse[_entities[idx]] = idx;
        _sparse.Remove(entity.Id);
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(Entity entity, out T component)
    {
        if (_sparse.TryGetValue(entity.Id, out var idx))
        {
            component = _dense[idx];
            return true;
        }

        component = default;
        return false;
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetComponentRef(Entity entity)
    {
        if (_sparse.TryGetValue(entity.Id, out var idx))
            return ref _dense[idx];

        throw new KeyNotFoundException();
    }

    IComponent ISparseSet.GetComponent(Entity entity)
        => (IComponent)GetComponentRef(entity);

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(Entity entity)
        => _sparse.ContainsKey(entity.Id);

    private void Grow()
    {
        Array.Resize(ref _dense, _dense.Length * 2);
        Array.Resize(ref _entities, _entities.Length * 2);
    }
}