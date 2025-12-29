namespace Lithium.Core.ECS;

public partial class World
{
    private readonly Dictionary<ArchetypeKey, Archetype> _archetypes = new();
    private readonly Dictionary<EntityId, Archetype> _entityArchetype = new();

    public Entity CreateEntity()
    {
        var entity = new Entity(++_nextEntityId);
        var key = new ArchetypeKey(ReadOnlySpan<Type>.Empty);

        if (!_archetypes.TryGetValue(key, out var archetype))
        {
            archetype = new Archetype();
            _archetypes[key] = archetype;
        }

        archetype.Entities.Add(entity);
        _entityArchetype[entity.Id] = archetype;

        return entity;
    }

    public void DestroyEntity(Entity entity)
    {
        if (!_entityArchetype.TryGetValue(entity.Id, out var archetype))
            return; // Entité inexistante

        // 1️⃣ Supprimer l'entité de l'archetype
        archetype.Entities.Remove(entity);

        // 2️⃣ Supprimer tous ses composants des SparseSets
        foreach (var (_, set) in archetype.Components)
        {
            // ISparseSet ne connaît pas T, donc on fait un cast générique via reflection
            var removeMethod = set.GetType().GetMethod("Remove", [typeof(Entity)]);
            removeMethod?.Invoke(set, [entity]);
        }

        // 3️⃣ Supprimer l'archetype mapping
        _entityArchetype.Remove(entity.Id);

        // 4️⃣ Optionnel : supprimer de la liste globale des entités
        // _entities.Remove(entity); // si tu gardes une liste globale
    }

    public void AddComponent<T>(Entity entity, T component)
        where T : struct
    {
        var oldArchetype = _entityArchetype[entity.Id];

        // TODO - Use Span<RuntimeTypeHandle> instead of Type[]
        // Construire la nouvelle signature
        var types = new Type[oldArchetype.Components.Count + 1];
        var i = 0;

        foreach (var t in oldArchetype.Components.Keys)
            types[i++] = t;

        types[i] = typeof(T);

        var newKey = new ArchetypeKey(types);

        if (!_archetypes.TryGetValue(newKey, out var newArchetype))
        {
            newArchetype = new Archetype();

            foreach (var t in types)
                newArchetype.Components[t] = GetOrCreateSetByType(t);

            _archetypes[newKey] = newArchetype;
        }

        // Migration
        oldArchetype.Entities.Remove(entity);
        newArchetype.Entities.Add(entity);

        _entityArchetype[entity.Id] = newArchetype;

        // Stockage du composant
        GetOrCreateSet<T>().Add(entity, component);
    }

    public void RemoveComponent<T>(Entity entity)
        where T : struct
    {
        var oldArchetype = _entityArchetype[entity.Id];

        // Si l'entité n'a pas ce composant, on sort
        if (!oldArchetype.Components.ContainsKey(typeof(T)))
            return;

        // Construire la nouvelle signature sans T
        var typesWithoutT = oldArchetype.Components.Keys
            .Where(type => type != typeof(T))
            .ToArray();

        var newKey = new ArchetypeKey(typesWithoutT);

        if (!_archetypes.TryGetValue(newKey, out var newArchetype))
        {
            newArchetype = new Archetype();

            foreach (var t in typesWithoutT)
                newArchetype.Components[t] = GetOrCreateSetByType(t);

            _archetypes[newKey] = newArchetype;
        }

        // Migration
        oldArchetype.Entities.Remove(entity);
        newArchetype.Entities.Add(entity);
        _entityArchetype[entity.Id] = newArchetype;

        // Suppression du composant
        GetOrCreateSet<T>().Remove(entity);
    }
}