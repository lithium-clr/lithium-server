using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 64,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 2,
    VariableFieldCount = 1,
    VariableBlockStart = 2,
    MaxSize = 1677721600
)]
public sealed class UpdateTranslationsPacket : INetworkSerializable
{
    [JsonPropertyName("type")]
    public UpdateType Type { get; set; } = UpdateType.Init;

    [JsonPropertyName("translations")]
    public Dictionary<string, string>? Translations { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Translations is not null) bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);

        if (Translations is not null)
        {
            writer.WriteVarInt(Translations.Count);
            foreach (var (key, value) in Translations)
            {
                writer.WriteVarUtf8String(key, 4096000);
                writer.WriteVarUtf8String(value, 4096000);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        Type = reader.ReadEnum<UpdateType>();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            Translations = new Dictionary<string, string>(count);
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadUtf8String();
                var value = reader.ReadUtf8String();
                Translations[key] = value;
            }
        }
    }
}