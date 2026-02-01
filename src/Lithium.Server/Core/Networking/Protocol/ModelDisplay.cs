using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ModelDisplay : INetworkSerializable
{
    [JsonPropertyName("node")] public string? Node { get; set; }
    [JsonPropertyName("attachTo")] public string? AttachTo { get; set; }
    [JsonPropertyName("translation")] public Vector3Float? Translation { get; set; }
    [JsonPropertyName("rotation")] public Vector3Float? Rotation { get; set; }
    [JsonPropertyName("scale")] public Vector3Float? Scale { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Translation is not null) bits.SetBit(1);
        if (Rotation is not null) bits.SetBit(2);
        if (Scale is not null) bits.SetBit(4);
        if (Node is not null) bits.SetBit(8);
        if (AttachTo is not null) bits.SetBit(16);

        writer.WriteBits(bits);

        if (Translation is not null)
        {
            Translation.Serialize(writer);
        }
        else
        {
            for (var i = 0; i < 12; i++)
                writer.WriteUInt8(0);
        }

        if (Rotation is not null)
        {
            Rotation.Serialize(writer);
        }
        else
        {
            for (var i = 0; i < 12; i++)
                writer.WriteUInt8(0);
        }

        if (Scale is not null)
        {
            Scale.Serialize(writer);
        }
        else
        {
            for (var i = 0; i < 12; i++)
                writer.WriteUInt8(0);
        }

        var nodeOffsetSlot = writer.ReserveOffset();
        var attachToOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Node is not null)
        {
            writer.WriteOffsetAt(nodeOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Node, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(nodeOffsetSlot, -1);
        }

        if (AttachTo is not null)
        {
            writer.WriteOffsetAt(attachToOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(AttachTo, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(attachToOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            Translation = reader.ReadObject<Vector3Float>();
        }
        // else
        // {
        //     for (var i = 0; i < 3; i++)
        //         reader.ReadFloat32();
        // }

        if (bits.IsSet(2))
        {
            Rotation = reader.ReadObject<Vector3Float>();
        }
        // else
        // {
        //     for (var i = 0; i < 3; i++)
        //         reader.ReadFloat32();
        // }

        if (bits.IsSet(4))
        {
            Scale = reader.ReadObject<Vector3Float>();
        }
        // else
        // {
        //     for (var i = 0; i < 3; i++)
        //         reader.ReadFloat32();
        // }

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(8))
            Node = reader.ReadVarUtf8StringAt(offsets[0]);

        if (bits.IsSet(16))
            AttachTo = reader.ReadVarUtf8StringAt(offsets[1]);
    }
}