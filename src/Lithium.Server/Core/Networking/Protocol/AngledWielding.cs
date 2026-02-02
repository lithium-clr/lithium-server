using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 0,
    FixedBlockSize = 9,
    VariableFieldCount = 0,
    VariableBlockStart = 9,
    MaxSize = 9
)]
public sealed class AngledWielding : INetworkSerializable
{
    [JsonPropertyName("angleRad")]         public float AngleRad         { get; set; }
    [JsonPropertyName("angleDistanceRad")] public float AngleDistanceRad { get; set; }
    [JsonPropertyName("hasModifiers")]     public bool  HasModifiers     { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(AngleRad);
        writer.WriteFloat32(AngleDistanceRad);
        writer.WriteBoolean(HasModifiers);
    }

    public void Deserialize(PacketReader reader)
    {
        AngleRad         = reader.ReadFloat32();
        AngleDistanceRad = reader.ReadFloat32();
        HasModifiers     = reader.ReadBoolean();
    }
}
