using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

public interface ISparseSet
{
    int Count { get; }
    ReadOnlySpan<EntityId> Entities { get; }

    IComponent GetComponent(Entity entity);
    bool Has(Entity entity);
    void Remove(Entity entity);
}

public sealed class SparseSet<T> : ISparseSet where T : struct
{
    private T[] _dense = new T[16];
    private EntityId[] _entities = new EntityId[16];
    private int[] _sparse = new int[1024];

    public SparseSet()
    {
        Array.Fill(_sparse, -1);
    }

    public int Count { get; private set; }
    public ReadOnlySpan<EntityId> Entities => _entities.AsSpan(0, Count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Entity entity, T component)
    {
        var id = (int)entity.Id;
        
        EnsureSparseSize(id);

        if (_sparse[id] != -1)
        {
            _dense[_sparse[id]] = component;
            return;
        }

        if (Count == _dense.Length)
            Grow();

        _dense[Count] = component;
        _entities[Count] = entity.Id;
        _sparse[id] = Count;
        Count++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Entity entity)
    {
        var id = (int)entity.Id;
        
        if (id >= _sparse.Length || _sparse[id] == -1)
            return;

        var denseIndex = _sparse[id];
        var lastIndex = --Count;
        var lastEntityId = _entities[lastIndex];

        _dense[denseIndex] = _dense[lastIndex];
        _entities[denseIndex] = lastEntityId;
        
        _sparse[(int)lastEntityId] = denseIndex;
        _sparse[id] = -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(Entity entity, out T component)
    {
        var id = (int)entity.Id;
        
        if (id < _sparse.Length)
        {
            var idx = _sparse[id];
            if (idx != -1)
            {
                component = _dense[idx];
                return true;
            }
        }

        component = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetComponentRef(Entity entity)
    {
        var id = (int)entity.Id;
        
        if (id < _sparse.Length)
        {
            var idx = _sparse[id];
            if (idx != -1)
                return ref _dense[idx];
        }

        throw new KeyNotFoundException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetIndex(Entity entity)
    {
        var id = (int)entity.Id;
        
        if (id < _sparse.Length)
            return _sparse[id];
        
        return -1;
    }

    IComponent ISparseSet.GetComponent(Entity entity)
        => (IComponent)GetComponentRef(entity);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(Entity entity)
    {
        var id = (int)entity.Id;
        
        return id < _sparse.Length && _sparse[id] != -1;
    }

    private void Grow()
    {
        Array.Resize(ref _dense, _dense.Length * 2);
        Array.Resize(ref _entities, _entities.Length * 2);
    }

    private void EnsureSparseSize(int id)
    {
        if (id < _sparse.Length) return;
        
        var newSize = Math.Max(id + 1, _sparse.Length * 2);
        var oldLength = _sparse.Length;
        Array.Resize(ref _sparse, newSize);
        _sparse.AsSpan(oldLength).Fill(-1);
    }
    
    public T[] GetDenseArray() => _dense;
    public int[] GetSparseArray() => _sparse;
}