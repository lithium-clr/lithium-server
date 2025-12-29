namespace Lithium.Core.ECS;

public interface ISparseSet
{
    int Count { get; }
    IReadOnlyList<EntityId> Entities { get; }

    bool Has(Entity entity);
}

public sealed class SparseSet<T> : ISparseSet where T : struct
{
    private readonly List<T> _dense = [];
    private readonly List<EntityId> _entities = [];
    private readonly Dictionary<EntityId, int> _sparse = [];

    public int Count => _dense.Count;
    public IReadOnlyList<T> Dense => _dense;
    public IReadOnlyList<EntityId> Entities => _entities;

    public void Add(Entity entity, T component)
    {
        if (_sparse.TryGetValue(entity.Id, out var idx))
        {
            _dense[idx] = component;
        }
        else
        {
            _sparse[entity.Id] = _dense.Count;
            _entities.Add(entity.Id);
            _dense.Add(component);
        }
    }

    public void Remove(Entity entity)
    {
        if (!_sparse.TryGetValue(entity.Id, out var idx)) return;

        var lastIdx = _dense.Count - 1;

        // Swap current <-> last
        _dense[idx] = _dense[lastIdx];
        _entities[idx] = _entities[lastIdx];
        _sparse[_entities[idx]] = idx;

        // Remove last
        _dense.RemoveAt(lastIdx);
        _entities.RemoveAt(lastIdx);
        _sparse.Remove(entity.Id);
    }

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

    public bool Has(Entity entity) => _sparse.ContainsKey(entity.Id);
}