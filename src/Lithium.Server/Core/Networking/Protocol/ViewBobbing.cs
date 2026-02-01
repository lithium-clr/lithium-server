using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 1,
    VariableFieldCount = 1,
    VariableBlockStart = 1,
    MaxSize = 565248085
)]
public sealed class ViewBobbing : INetworkSerializable
{
    [JsonPropertyName("firstPerson")] public CameraShakeConfig? FirstPerson { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (FirstPerson is not null)
        {
            bits.SetBit(1);
        }

        writer.WriteBits(bits);

        if (FirstPerson is not null)
        {
            FirstPerson.Serialize(writer);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            FirstPerson = reader.ReadObject<CameraShakeConfig>();
        }
    }
}