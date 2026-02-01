using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ItemGlider : INetworkSerializable
{
    [JsonPropertyName("terminalVelocity")]
    public float TerminalVelocity { get; set; }

    [JsonPropertyName("fallSpeedMultiplier")]
    public float FallSpeedMultiplier { get; set; }

    [JsonPropertyName("horizontalSpeedMultiplier")]
    public float HorizontalSpeedMultiplier { get; set; }

    [JsonPropertyName("speed")]
    public float Speed { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(TerminalVelocity);
        writer.WriteFloat32(FallSpeedMultiplier);
        writer.WriteFloat32(HorizontalSpeedMultiplier);
        writer.WriteFloat32(Speed);
    }

    public void Deserialize(PacketReader reader)
    {
        TerminalVelocity = reader.ReadFloat32();
        FallSpeedMultiplier = reader.ReadFloat32();
        HorizontalSpeedMultiplier = reader.ReadFloat32();
        Speed = reader.ReadFloat32();
    }
}
