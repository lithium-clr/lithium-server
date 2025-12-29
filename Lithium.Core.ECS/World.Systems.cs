namespace Lithium.Core.ECS;

public partial class World
{
    private readonly List<ISystem> _systems = [];

    public IReadOnlyList<ISystem> Systems => _systems;

    public void Update(float deltaTime)
    {
        foreach (var system in _systems)
            system.Update(this, deltaTime);
    }

    public void AddSystem(ISystem system) => _systems.Add(system);
    public void RemoveSystem(ISystem system) => _systems.Remove(system);
    public void ClearSystems() => _systems.Clear();
}