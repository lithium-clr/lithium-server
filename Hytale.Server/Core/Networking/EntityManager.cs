namespace Hytale.Server.Core.Networking;

public interface IEntityManager
{
    IReadOnlyList<Entity> Entities { get; }
    
    Entity? Get(int entityId);
}

public sealed class EntityManager : IEntityManager
{
    private readonly List<Entity> _entities = [];

    public IReadOnlyList<Entity> Entities => _entities;

    public void RegisterEntity(Entity entity)
    {
        entity.Id = Entities.Count + 1;
        _entities.Add(entity);
    }

    public void UnregisterEntity(Entity entity)
    {
        _entities.Remove(entity);
    }

    public Entity? Get(int entityId)
    {
        return _entities.FirstOrDefault(x => x.Id == entityId);
    }
}