namespace Lithium.Core.ECS;

public partial class World
{
    private int _nextEntityId;

    public Entity CreateEntity()
    {
        var entity = new Entity(this, ++_nextEntityId);

        EnsureEntityTagsSize((int)_nextEntityId);
        EnsureEntityArchetypesSize((int)_nextEntityId);

        if (!_archetypes.TryGetValue(ArchetypeKey.Empty, out var archetype))
        {
            archetype = new Archetype(4);
            _archetypes[ArchetypeKey.Empty] = archetype;
        }

        archetype.Add(entity);
        _entityArchetypes[entity.Id] = archetype;

        return entity;
    }
}