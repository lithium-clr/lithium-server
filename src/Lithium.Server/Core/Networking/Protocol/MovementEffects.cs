using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 0,
    FixedBlockSize = 7,
    VariableFieldCount = 0,
    VariableBlockStart = 7,
    MaxSize = 7)
]
public sealed class MovementEffects : INetworkSerializable
{
    [JsonPropertyName("disableForward")] public bool DisableForward { get; set; }
    [JsonPropertyName("disableBackward")] public bool DisableBackward { get; set; }
    [JsonPropertyName("disableLeft")] public bool DisableLeft { get; set; }
    [JsonPropertyName("disableRight")] public bool DisableRight { get; set; }
    [JsonPropertyName("disableSprint")] public bool DisableSprint { get; set; }
    [JsonPropertyName("disableJump")] public bool DisableJump { get; set; }
    [JsonPropertyName("disableCrouch")] public bool DisableCrouch { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteBoolean(DisableForward);
        writer.WriteBoolean(DisableBackward);
        writer.WriteBoolean(DisableLeft);
        writer.WriteBoolean(DisableRight);
        writer.WriteBoolean(DisableSprint);
        writer.WriteBoolean(DisableJump);
        writer.WriteBoolean(DisableCrouch);
    }

    public void Deserialize(PacketReader reader)
    {
        DisableForward = reader.ReadBoolean();
        DisableBackward = reader.ReadBoolean();
        DisableLeft = reader.ReadBoolean();
        DisableRight = reader.ReadBoolean();
        DisableSprint = reader.ReadBoolean();
        DisableJump = reader.ReadBoolean();
        DisableCrouch = reader.ReadBoolean();
    }
}