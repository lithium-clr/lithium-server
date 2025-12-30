namespace Lithium.Core.ECS;

public partial class World
{
    private uint _nextEntityId;

    public Entity CreateEntity()
    {
        var entity = new Entity(this, ++_nextEntityId);

        var archetype = _archetypes.TryGetValue(ArchetypeKey.Empty, out var a)
            ? a
            : _archetypes[ArchetypeKey.Empty] = new Archetype(4);

        archetype.Add(entity);
        _entityArchetype[entity.Id] = archetype;

        return entity;
    }
}