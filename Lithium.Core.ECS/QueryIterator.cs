namespace Lithium.Core.ECS;

public interface IQueryIterator<out T>
{
    bool MoveNext();
    T Current { get; }
}

public ref struct QueryIterator<T1, T2>
    where T1 : struct
    where T2 : struct
{
    private readonly SparseSet<T1> _set1;
    private readonly SparseSet<T2> _set2;
    private readonly IReadOnlyList<EntityId> _entities;

    private int _index;
    private Entity _entity;
    private T1 _c1;
    private T2 _c2;
    
    public readonly (Entity, T1, T2) Current
        => (_entity, _c1, _c2);

    public QueryIterator(SparseSet<T1> set1, SparseSet<T2> set2)
    {
        _set1 = set1;
        _set2 = set2;

        _entities = set1.Count <= set2.Count
            ? set1.Entities
            : set2.Entities;

        _index = -1;
        _entity = default;
        _c1 = default;
        _c2 = default;
    }

    public bool MoveNext()
    {
        while (++_index < _entities.Count)
        {
            _entity = new Entity(_entities[_index]);

            if (_set1.TryGet(_entity, out _c1) &&
                _set2.TryGet(_entity, out _c2))
                return true;
        }

        return false;
    }
}