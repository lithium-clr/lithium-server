using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 2,
    VariableFieldCount = 1,
    VariableBlockStart = 2,
    MaxSize = 32768028
)]
public sealed class RootInteractionSettings : INetworkSerializable
{
    [JsonPropertyName("allowSkipChainOnClick")]
    public bool AllowSkipChainOnClick { get; set; }

    [JsonPropertyName("cooldown")]
    public InteractionCooldown? Cooldown { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Cooldown is not null) bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteBoolean(AllowSkipChainOnClick);

        if (Cooldown is not null)
        {
            Cooldown.Serialize(writer);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        AllowSkipChainOnClick = reader.ReadBoolean();

        if (bits.IsSet(1))
        {
            Cooldown = new InteractionCooldown();
            Cooldown.Deserialize(reader);
        }
    }
}