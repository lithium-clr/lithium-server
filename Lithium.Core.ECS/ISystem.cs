namespace Lithium.Core.ECS;

public interface ISystem
{
    World World { get; }
    float DeltaTime { get; }
    
    void OnUpdate();
}

public abstract class System : ISystem
{
    public World World { get; internal set; } = null!;
    public float DeltaTime { get; internal set; }
    
    public abstract void OnUpdate();
}