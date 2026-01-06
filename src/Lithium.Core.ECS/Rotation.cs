namespace Lithium.Core.ECS;

public record struct Rotation(float Pitch, float Yaw, float Roll) : IComponent
{
    public override string ToString() => $"{nameof(Rotation)}({Pitch}, {Yaw}, {Roll})";
}