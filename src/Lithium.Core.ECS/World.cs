namespace Lithium.Core.ECS;

public sealed partial class World
{
    public World()
    {
        _archetypes[ArchetypeKey.Empty] = Archetype.Empty;
    }
}