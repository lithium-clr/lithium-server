using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 0,
    FixedBlockSize = 1,
    VariableFieldCount = 0,
    VariableBlockStart = 1,
    MaxSize = 1)
]
public sealed class InteractionSettings : INetworkSerializable
{
    [JsonPropertyName("allowSkipOnClick")] public bool AllowSkipOnClick { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteBoolean(AllowSkipOnClick);
    }

    public void Deserialize(PacketReader reader)
    {
        AllowSkipOnClick = reader.ReadBoolean();
    }
}