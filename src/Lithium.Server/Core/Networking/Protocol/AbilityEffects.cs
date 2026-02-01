using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class AbilityEffects : INetworkSerializable
{
    [JsonPropertyName("disabled")]
    public InteractionType[]? Disabled { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Disabled is not null)
        {
            bits.SetBit(1);
        }
        writer.WriteBits(bits);

        if (Disabled is not null)
        {
            writer.WriteVarInt(Disabled.Length);
            foreach (var item in Disabled)
            {
                writer.WriteEnum(item);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            Disabled = new InteractionType[count];
            for (var i = 0; i < count; i++)
            {
                Disabled[i] = reader.ReadEnum<InteractionType>();
            }
        }
    }
}
