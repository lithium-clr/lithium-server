using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ItemTool : INetworkSerializable
{
    [JsonPropertyName("specs")]
    public ItemToolSpec[]? Specs { get; set; }

    [JsonPropertyName("speed")]
    public float Speed { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Specs is not null)
        {
            bits.SetBit(1);
        }
        writer.WriteBits(bits);

        writer.WriteFloat32(Speed);

        if (Specs is not null)
        {
            writer.WriteVarInt(Specs.Length);
            foreach (var spec in Specs)
            {
                spec.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        Speed = reader.ReadFloat32();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            Specs = new ItemToolSpec[count];
            for (var i = 0; i < count; i++)
            {
                Specs[i] = reader.ReadObject<ItemToolSpec>();
            }
        }
    }
}
