using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

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
                item.Serialize(writer);
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
            FaceType = reader.ReadVarUtf8StringAt(offsets[0]);

        if (bits.IsSet(2))
        {
            Filler = reader.ReadArrayAt(
                offsets[1],
                r => r.ReadObject<Vector3Int>()
            );
        }
    }
}