using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 40, IsCompressed = true, NullableBitFieldSize = 1, FixedBlockSize = 10, VariableFieldCount = 1,
    VariableBlockStart = 10, MaxSize = 1677721600)]
public sealed class UpdateBlockTypesPacket : INetworkSerializable
{
    [JsonPropertyName("type")] 
    public UpdateType Type { get; set; } = UpdateType.Init;
    
    [JsonPropertyName("maxId")] 
    public int MaxId { get; set; }
    
    [JsonPropertyName("blockTypes")] 
    public Dictionary<int, BlockType>? BlockTypes { get; set; }
    
    [JsonPropertyName("updateBlockTextures")] 
    public bool UpdateBlockTextures { get; set; }
    
    [JsonPropertyName("updateModelTextures")] 
    public bool UpdateModelTextures { get; set; }
    
    [JsonPropertyName("updateModels")] 
    public bool UpdateModels { get; set; }
    
    [JsonPropertyName("updateMapGeometry")] 
    public bool UpdateMapGeometry { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (BlockTypes is not null)
            bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);
        writer.WriteInt32(MaxId);
        writer.WriteBoolean(UpdateBlockTextures);
        writer.WriteBoolean(UpdateModelTextures);
        writer.WriteBoolean(UpdateModels);
        writer.WriteBoolean(UpdateMapGeometry);

        if (BlockTypes is not null)
        {
            writer.WriteVarInt(BlockTypes.Count);

            foreach (var item in BlockTypes)
            {
                writer.WriteInt32(item.Key);
                item.Value.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Type = reader.ReadEnum<UpdateType>();
        MaxId = reader.ReadInt32();
        UpdateBlockTextures = reader.ReadBoolean();
        UpdateModelTextures = reader.ReadBoolean();
        UpdateModels = reader.ReadBoolean();
        UpdateMapGeometry = reader.ReadBoolean();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            BlockTypes = new Dictionary<int, BlockType>(count);

            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadInt32();
                var value = reader.ReadObject<BlockType>();
                BlockTypes[key] = value;
            }
        }
    }
}