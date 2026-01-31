using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class BlockMovementSettings : INetworkSerializable
{
    [JsonPropertyName("isClimbable")] public bool IsClimbable { get; set; }

    [JsonPropertyName("climbUpSpeedMultiplier")]
    public float ClimbUpSpeedMultiplier { get; set; }

    [JsonPropertyName("climbDownSpeedMultiplier")]
    public float ClimbDownSpeedMultiplier { get; set; }

    [JsonPropertyName("climbLateralSpeedMultiplier")]
    public float ClimbLateralSpeedMultiplier { get; set; }

    [JsonPropertyName("isBouncy")] public bool IsBouncy { get; set; }
    [JsonPropertyName("bounceVelocity")] public float BounceVelocity { get; set; }
    [JsonPropertyName("drag")] public float Drag { get; set; }
    [JsonPropertyName("friction")] public float Friction { get; set; }

    [JsonPropertyName("terminalVelocityModifier")]
    public float TerminalVelocityModifier { get; set; }

    [JsonPropertyName("horizontalSpeedMultiplier")]
    public float HorizontalSpeedMultiplier { get; set; }

    [JsonPropertyName("acceleration")] public float Acceleration { get; set; }

    [JsonPropertyName("jumpForceMultiplier")]
    public float JumpForceMultiplier { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteBoolean(IsClimbable);
        writer.WriteFloat32(ClimbUpSpeedMultiplier);
        writer.WriteFloat32(ClimbDownSpeedMultiplier);
        writer.WriteFloat32(ClimbLateralSpeedMultiplier);
        writer.WriteBoolean(IsBouncy);
        writer.WriteFloat32(BounceVelocity);
        writer.WriteFloat32(Drag);
        writer.WriteFloat32(Friction);
        writer.WriteFloat32(TerminalVelocityModifier);
        writer.WriteFloat32(HorizontalSpeedMultiplier);
        writer.WriteFloat32(Acceleration);
        writer.WriteFloat32(JumpForceMultiplier);
    }

    public void Deserialize(PacketReader reader)
    {
        IsClimbable = reader.ReadBoolean();
        ClimbUpSpeedMultiplier = reader.ReadFloat32();
        ClimbDownSpeedMultiplier = reader.ReadFloat32();
        ClimbLateralSpeedMultiplier = reader.ReadFloat32();
        IsBouncy = reader.ReadBoolean();
        BounceVelocity = reader.ReadFloat32();
        Drag = reader.ReadFloat32();
        Friction = reader.ReadFloat32();
        TerminalVelocityModifier = reader.ReadFloat32();
        HorizontalSpeedMultiplier = reader.ReadFloat32();
        Acceleration = reader.ReadFloat32();
        JumpForceMultiplier = reader.ReadFloat32();
    }
}