using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ItemReticleConfig : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("base")]
    public string[]? Base { get; set; }

    [JsonPropertyName("serverEvents")]
    public Dictionary<int, ItemReticle>? ServerEvents { get; set; }

    [JsonPropertyName("clientEvents")]
    public Dictionary<ItemReticleClientEvent, ItemReticle>? ClientEvents { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Id is not null) bits.SetBit(1);
        if (Base is not null) bits.SetBit(2);
        if (ServerEvents is not null) bits.SetBit(4);
        if (ClientEvents is not null) bits.SetBit(8);
        writer.WriteBits(bits);

        var idOffsetSlot = writer.ReserveOffset();
        var baseOffsetSlot = writer.ReserveOffset();
        var serverEventsOffsetSlot = writer.ReserveOffset();
        var clientEventsOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        writer.WriteOffsetAt(idOffsetSlot, Id is not null ? writer.Position - varBlockStart : -1);
        if (Id is not null) writer.WriteVarUtf8String(Id, 4096000);

        writer.WriteOffsetAt(baseOffsetSlot, Base is not null ? writer.Position - varBlockStart : -1);
        if (Base is not null)
        {
            writer.WriteVarInt(Base.Length);
            foreach (var item in Base)
            {
                writer.WriteVarUtf8String(item, 4096000);
            }
        }

        writer.WriteOffsetAt(serverEventsOffsetSlot, ServerEvents is not null ? writer.Position - varBlockStart : -1);
        if (ServerEvents is not null)
        {
            writer.WriteVarInt(ServerEvents.Count);
            foreach (var (key, value) in ServerEvents)
            {
                writer.WriteInt32(key);
                value.Serialize(writer);
            }
        }

        writer.WriteOffsetAt(clientEventsOffsetSlot, ClientEvents is not null ? writer.Position - varBlockStart : -1);
        if (ClientEvents is not null)
        {
            writer.WriteVarInt(ClientEvents.Count);
            foreach (var (key, value) in ClientEvents)
            {
                writer.WriteEnum(key);
                value.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());
        var offsets = reader.ReadOffsets(4);
        var currentPos = reader.GetPosition();

        if (bits.IsSet(1)) Id = reader.ReadVarUtf8StringAt(offsets[0]);

        if (bits.IsSet(2))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[1]);
            var count = reader.ReadVarInt32();
            Base = new string[count];
            for (var i = 0; i < count; i++)
            {
                Base[i] = reader.ReadUtf8String();
            }
        }

        if (bits.IsSet(4))
        {
            ServerEvents = reader.ReadDictionaryAt(
                offsets[2],
                r => r.ReadInt32(),
                r => r.ReadObject<ItemReticle>()
            );
        }

        if (bits.IsSet(8))
        {
            ClientEvents = reader.ReadDictionaryAt(
                offsets[3],
                r => r.ReadEnum<ItemReticleClientEvent>(),
                r => r.ReadObject<ItemReticle>()
            );
        }
        
        reader.SeekTo(currentPos);
    }
}
