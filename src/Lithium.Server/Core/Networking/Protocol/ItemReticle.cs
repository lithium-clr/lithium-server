using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ItemReticle : INetworkSerializable
{
    [JsonPropertyName("hideBase")]
    public bool HideBase { get; set; }

    [JsonPropertyName("parts")]
    public string[]? Parts { get; set; }

    [JsonPropertyName("duration")]
    public float Duration { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Parts is not null)
        {
            bits.SetBit(1);
        }
        writer.WriteBits(bits);

        writer.WriteBoolean(HideBase);
        writer.WriteFloat32(Duration);

        if (Parts is not null)
        {
            writer.WriteVarInt(Parts.Length);
            foreach (var part in Parts)
            {
                writer.WriteVarUtf8String(part, 4096000);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        HideBase = reader.ReadBoolean();
        Duration = reader.ReadFloat32();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            Parts = new string[count];
            for (var i = 0; i < count; i++)
            {
                Parts[i] = reader.ReadUtf8String();
            }
        }
    }
}
