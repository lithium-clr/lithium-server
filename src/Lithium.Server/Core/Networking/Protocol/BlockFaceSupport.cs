using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 1,
    VariableFieldCount = 2,
    VariableBlockStart = 9,
    MaxSize = 65536019
)]
public sealed class BlockFaceSupport : INetworkSerializable
{
    [JsonPropertyName("faceType")] public string? FaceType { get; set; }
    [JsonPropertyName("filler")] public Vector3Int[]? Filler { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (FaceType is not null) bits.SetBit(1);
        if (Filler is not null) bits.SetBit(2);

        writer.WriteBits(bits);

        var faceTypeOffsetSlot = writer.ReserveOffset();
        var fillerOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (FaceType is not null)
        {
            writer.WriteOffsetAt(faceTypeOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(FaceType, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(faceTypeOffsetSlot, -1);
        }

        if (Filler is not null)
        {
            writer.WriteOffsetAt(fillerOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Filler.Length);
            foreach (var item in Filler)
            {
                item.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(fillerOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            FaceType = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            var count = reader.ReadVarIntAt(offsets[1], out var bytesRead);
            var pos = offsets[1] + bytesRead;

            Filler = new Vector3Int[count];
            for (var i = 0; i < count; i++)
            {
                Filler[i] = reader.ReadObjectAt<Vector3Int>(pos);
                pos += 12; // Vector3Int is 3x4 bytes fixed size
            }
        }
    }
}