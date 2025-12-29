namespace Lithium.Core.ECS;

public sealed partial class World
{
    private uint _nextEntityId;

    private readonly Dictionary<Type, object> _components = new();

    private SparseSet<T> GetOrCreateSet<T>() where T : struct
    {
        if (_components.TryGetValue(typeof(T), out var obj))
            return (SparseSet<T>)obj;

        var set = new SparseSet<T>();
        _components[typeof(T)] = set;
        return set;
    }

    private ISparseSet GetOrCreateSetByType(Type type)
    {
        if (_components.TryGetValue(type, out var obj))
            return (ISparseSet)obj;

        var setType = typeof(SparseSet<>).MakeGenericType(type);
        var set = (ISparseSet)Activator.CreateInstance(setType)!;
        _components[type] = set;
        return set;
    }

    public bool TryGetComponent<T>(Entity entity, out T component)
        where T : struct
    {
        if (_components.TryGetValue(typeof(T), out var obj))
            return ((SparseSet<T>)obj).TryGet(entity, out component);

        component = default;
        return false;
    }

    // ===== Archetype Queries (O(n) direct) =====

    public IReadOnlyList<Entity> QueryArchetype(params Type[] componentTypes)
    {
        var key = new ArchetypeKey(componentTypes);

        return _archetypes.TryGetValue(key, out var archetype)
            ? archetype.Entities
            : Array.Empty<Entity>();
    }

    // ===== Typed Wrappers =====

    public IEnumerable<(Entity, T1)> Query<T1>()
        where T1 : struct
    {
        foreach (var e in QueryArchetype(typeof(T1)))
        {
            TryGetComponent(e, out T1 c1);
            yield return (e, c1);
        }
    }

    public IEnumerable<(Entity, T1, T2)> Query<T1, T2>()
        where T1 : struct
        where T2 : struct
    {
        foreach (var e in QueryArchetype(typeof(T1), typeof(T2)))
        {
            TryGetComponent(e, out T1 c1);
            TryGetComponent(e, out T2 c2);
            
            yield return (e, c1, c2);
        }
    }

    public IEnumerable<(Entity, T1, T2, T3)> Query<T1, T2, T3>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        foreach (var e in QueryArchetype(typeof(T1), typeof(T2), typeof(T3)))
        {
            TryGetComponent(e, out T1 c1);
            TryGetComponent(e, out T2 c2);
            TryGetComponent(e, out T3 c3);
            
            yield return (e, c1, c2, c3);
        }
    }
}