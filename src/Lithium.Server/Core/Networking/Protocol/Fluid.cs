using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 22,
    VariableFieldCount = 5,
    VariableBlockStart = 42,
    MaxSize = 1677721600
)]
public sealed class Fluid : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("maxFluidLevel")]
    public int MaxFluidLevel { get; set; }

    [JsonPropertyName("cubeTextures")]
    public BlockTextures[]? CubeTextures { get; set; }

    [JsonPropertyName("requiresAlphaBlending")]
    public bool RequiresAlphaBlending { get; set; }

    [JsonPropertyName("opacity")]
    [JsonConverter(typeof(JsonStringEnumConverter<Opacity>))]
    public Opacity Opacity { get; set; } = Opacity.Solid;

    [JsonPropertyName("shaderEffect")]
    public ShaderType[]? ShaderEffect { get; set; }

    [JsonPropertyName("light")]
    public ColorLight? Light { get; set; }

    [JsonPropertyName("fluidFXIndex")]
    public int FluidFxIndex { get; set; }

    [JsonPropertyName("blockSoundSetIndex")]
    public int BlockSoundSetIndex { get; set; }

    [JsonPropertyName("blockParticleSetId")]
    public string? BlockParticleSetId { get; set; }

    [JsonPropertyName("particleColor")]
    public Color? ParticleColor { get; set; }

    [JsonPropertyName("tagIndexes")]
    public int[]? TagIndexes { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Light is not null) bits.SetBit(1);
        if (ParticleColor is not null) bits.SetBit(2);
        if (Id is not null) bits.SetBit(4);
        if (CubeTextures is not null) bits.SetBit(8);
        if (ShaderEffect is not null) bits.SetBit(16);
        if (BlockParticleSetId is not null) bits.SetBit(32);
        if (TagIndexes is not null) bits.SetBit(64);

        writer.WriteBits(bits);

        // Fixed Block
        writer.WriteInt32(MaxFluidLevel);
        writer.WriteBoolean(RequiresAlphaBlending);
        writer.WriteEnum(Opacity);
        
        if (Light is not null) Light.Serialize(writer); 
        else writer.WriteZero(4);
        
        writer.WriteInt32(FluidFxIndex);
        writer.WriteInt32(BlockSoundSetIndex);
        
        if (ParticleColor is not null) ParticleColor.Serialize(writer);
        else writer.WriteZero(3);

        // Reserve Offsets
        var idOffset = writer.ReserveOffset();
        var cubeTexturesOffset = writer.ReserveOffset();
        var shaderEffectOffset = writer.ReserveOffset();
        var blockParticleSetIdOffset = writer.ReserveOffset();
        var tagIndexesOffset = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // Variable Block
        if (Id is not null)
        {
            writer.WriteOffsetAt(idOffset, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Id, 4096000);
        }
        else writer.WriteOffsetAt(idOffset, -1);

        if (CubeTextures is not null)
        {
            writer.WriteOffsetAt(cubeTexturesOffset, writer.Position - varBlockStart);
            writer.WriteVarInt(CubeTextures.Length);
            foreach (var item in CubeTextures) item.Serialize(writer);
        }
        else writer.WriteOffsetAt(cubeTexturesOffset, -1);

        if (ShaderEffect is not null)
        {
            writer.WriteOffsetAt(shaderEffectOffset, writer.Position - varBlockStart);
            writer.WriteVarInt(ShaderEffect.Length);
            foreach (var item in ShaderEffect) writer.WriteEnum(item);
        }
        else writer.WriteOffsetAt(shaderEffectOffset, -1);

        if (BlockParticleSetId is not null)
        {
            writer.WriteOffsetAt(blockParticleSetIdOffset, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(BlockParticleSetId, 4096000);
        }
        else writer.WriteOffsetAt(blockParticleSetIdOffset, -1);

        if (TagIndexes is not null)
        {
            writer.WriteOffsetAt(tagIndexesOffset, writer.Position - varBlockStart);
            writer.WriteVarInt(TagIndexes.Length);
            foreach (var item in TagIndexes) writer.WriteInt32(item);
        }
        else writer.WriteOffsetAt(tagIndexesOffset, -1);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        // Fixed Block
        MaxFluidLevel = reader.ReadInt32();
        RequiresAlphaBlending = reader.ReadBoolean();
        Opacity = reader.ReadEnum<Opacity>();
        
        if (bits.IsSet(1)) 
            Light = reader.ReadObject<ColorLight>();
        
        FluidFxIndex = reader.ReadInt32();
        BlockSoundSetIndex = reader.ReadInt32();
        
        if (bits.IsSet(2)) 
            ParticleColor = reader.ReadObject<Color>();

        // Read Offsets
        var offsets = reader.ReadOffsets(5);

        // Variable Block
        if (bits.IsSet(4)) Id = reader.ReadVarUtf8StringAt(offsets[0]);
        if (bits.IsSet(8)) CubeTextures = reader.ReadObjectArrayAt<BlockTextures>(offsets[1]);
        if (bits.IsSet(16)) ShaderEffect = reader.ReadArrayAt(offsets[2], r => r.ReadEnum<ShaderType>());
        if (bits.IsSet(32)) BlockParticleSetId = reader.ReadVarUtf8StringAt(offsets[3]);
        if (bits.IsSet(64)) TagIndexes = reader.ReadArrayAt(offsets[4], r => r.ReadInt32());
    }
}
