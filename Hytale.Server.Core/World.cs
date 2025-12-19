namespace Hytale.Server.Core;

public interface IWorldService
{
    World Current { get; }

    void SetWorld(World world);
}

public sealed class World
{
    private static IWorldService _worldService = null!;
    private readonly List<Entity> _entities = [];

    public IReadOnlyList<Entity> Entities => _entities;

    public static World Current => _worldService.Current;

    internal static void Init(IWorldService worldService)
        => _worldService = worldService;

    public static void Set(World world)
    {
        _worldService.SetWorld(world);
    }

    public Client? GetPlayer(int serverId)
    {
        return _entities.OfType<Client>().FirstOrDefault(x => x.ServerId == serverId);
    }
}