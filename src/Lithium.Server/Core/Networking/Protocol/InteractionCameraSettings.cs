using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 1,
    VariableFieldCount = 2,
    VariableBlockStart = 9,
    MaxSize = 237568019)
]
public sealed class InteractionCameraSettings : INetworkSerializable
{
    [JsonPropertyName("firstPerson")]
    public InteractionCamera[]? FirstPerson { get; set; }

    [JsonPropertyName("thirdPerson")]
    public InteractionCamera[]? ThirdPerson { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (FirstPerson is not null) bits.SetBit(1);
        if (ThirdPerson is not null) bits.SetBit(2);

        writer.WriteBits(bits);

        var firstPersonOffsetSlot = writer.ReserveOffset();
        var thirdPersonOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (FirstPerson is not null)
        {
            writer.WriteOffsetAt(firstPersonOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(FirstPerson.Length);
            foreach (var item in FirstPerson)
            {
                item.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(firstPersonOffsetSlot, -1);
        }

        if (ThirdPerson is not null)
        {
            writer.WriteOffsetAt(thirdPersonOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(ThirdPerson.Length);
            foreach (var item in ThirdPerson)
            {
                item.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(thirdPersonOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            FirstPerson = reader.ReadArrayAt(offsets[0], r => r.ReadObject<InteractionCamera>());
        }

        if (bits.IsSet(2))
        {
            ThirdPerson = reader.ReadArrayAt(offsets[1], r => r.ReadObject<InteractionCamera>());
        }
    }
}