namespace Lithium.Core.ECS;

public interface ISystem
{
    void Update(World world, float deltaTime);
}