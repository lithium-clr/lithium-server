using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

public partial class World
{
    private readonly Dictionary<Type, ISparseSet> _components = new();

    public void AddComponent<T>(Entity entity, T component) where T : struct, IComponent
    {
        var set = GetSet<T>();

        if (set.Has(entity))
        {
            set.GetComponentRef(entity) = component;
            return;
        }

        set.Add(entity, component);

        if (!_entityArchetype.TryGetValue(entity.Id, out var oldArchetype))
        {
            oldArchetype = _archetypes.GetValueOrDefault(ArchetypeKey.Empty, Archetype.Empty);
            _entityArchetype[entity.Id] = oldArchetype;
        }

        var newKey = oldArchetype.GetKeyWith(typeof(T));

        if (!_archetypes.TryGetValue(newKey, out var newArchetype))
        {
            newArchetype = new Archetype();

            foreach (var type in oldArchetype.ComponentTypes)
                newArchetype.ComponentTypes.Add(type);

            newArchetype.ComponentTypes.Add(typeof(T)); 
            
            _archetypes[newKey] = newArchetype;
        }

        if (oldArchetype != newArchetype)
        {
            oldArchetype.Remove(entity);
            newArchetype.Add(entity);
            _entityArchetype[entity.Id] = newArchetype;
        }
    }

    public void RemoveComponent<T>(Entity entity) where T : struct, IComponent
    {
        GetSet<T>().Remove(entity);

        var oldArchetype = _entityArchetype[entity.Id];
        var newKey = oldArchetype.GetKeyWithout(typeof(T));

        if (!_archetypes.TryGetValue(newKey, out var newArchetype))
            _archetypes[newKey] = newArchetype = new Archetype();

        oldArchetype.Remove(entity);
        newArchetype.Add(entity);
        _entityArchetype[entity.Id] = newArchetype;
    }

    public bool TryGetComponent<T>(Entity e, out T component)
        where T : struct, IComponent
        => GetSet<T>().TryGet(e, out component);

    private SparseSet<T> GetSet<T>() where T : struct, IComponent
    {
        if (!_components.TryGetValue(typeof(T), out var set))
        {
            var created = new SparseSet<T>();
            _components[typeof(T)] = created;
            return created;
        }

        return (SparseSet<T>)set;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetComponentRef<T>(Entity entity)
        where T : struct, IComponent
    {
        return ref ((SparseSet<T>)_components[typeof(T)]).GetComponentRef(entity);
    }
}
