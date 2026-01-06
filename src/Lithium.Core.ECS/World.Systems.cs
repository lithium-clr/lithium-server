namespace Lithium.Core.ECS;

public partial class World
{
    private readonly List<System> _systems = [];

    public IReadOnlyList<ISystem> Systems => _systems;

    public void Update(float deltaTime)
    {
        foreach (var system in _systems)
        {
            system.World = this;
            system.DeltaTime = deltaTime;
            system.OnUpdate();
        }
    }

    public void AddSystem(System system) => _systems.Add(system);
    public void RemoveSystem(System system) => _systems.Remove(system);
    public void ClearSystems() => _systems.Clear();
}