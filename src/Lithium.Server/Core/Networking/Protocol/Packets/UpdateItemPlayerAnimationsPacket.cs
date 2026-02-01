using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 52, IsCompressed = true, NullableBitFieldSize = 1, FixedBlockSize = 2, VariableFieldCount = 1,
    VariableBlockStart = 2, MaxSize = 1677721600)]
public sealed class UpdateItemPlayerAnimationsPacket : INetworkSerializable
{
    [JsonPropertyName("type")] 
    public UpdateType Type { get; set; } = UpdateType.Init;

    [JsonPropertyName("itemPlayerAnimations")]
    public Dictionary<string, ItemPlayerAnimations>? ItemPlayerAnimations { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (ItemPlayerAnimations is not null)
            bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);

        if (ItemPlayerAnimations is not null)
        {
            writer.WriteVarInt(ItemPlayerAnimations.Count);

            foreach (var entry in ItemPlayerAnimations)
            {
                writer.WriteVarUtf8String(entry.Key, 4096000);
                entry.Value.Serialize(writer);
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
            ItemPlayerAnimations = new Dictionary<string, ItemPlayerAnimations>(count);

            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadUtf8String();
                var value = reader.ReadObject<ItemPlayerAnimations>();
                
                ItemPlayerAnimations[key] = value;
            }
        }
    }
}