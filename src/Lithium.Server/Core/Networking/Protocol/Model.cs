using System.Text.Json.Serialization;
using Lithium.Server.Core.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 2,
    FixedBlockSize = 43,
    VariableFieldCount = 12,
    VariableBlockStart = 91,
    MaxSize = 1677721600
)]
public sealed class Model : INetworkSerializable
{
    [JsonPropertyName("assetId")] public string? AssetId { get; set; }
    [JsonPropertyName("path")] public string? Path { get; set; }
    [JsonPropertyName("texture")] public string? Texture { get; set; }
    [JsonPropertyName("gradientSet")] public string? GradientSet { get; set; }
    [JsonPropertyName("gradientId")] public string? GradientId { get; set; }
    [JsonPropertyName("camera")] public CameraSettings? Camera { get; set; }
    [JsonPropertyName("scale")] public float Scale { get; set; }
    [JsonPropertyName("eyeHeight")] public float EyeHeight { get; set; }
    [JsonPropertyName("crouchOffset")] public float CrouchOffset { get; set; }

    [JsonPropertyName("animationSets")]
    public Dictionary<string, AnimationSet>? AnimationSets { get; set; }

    [JsonPropertyName("attachments")] public ModelAttachment[]? Attachments { get; set; }
    [JsonPropertyName("hitbox")] public Hitbox? Hitbox { get; set; }
    [JsonPropertyName("particles")] public ModelParticle[]? Particles { get; set; }
    [JsonPropertyName("trails")] public ModelTrail[]? Trails { get; set; }
    [JsonPropertyName("light")] public ColorLight? Light { get; set; }

    [JsonPropertyName("detailBoxes")]
    public Dictionary<string, DetailBox[]>? DetailBoxes { get; set; }

    [JsonPropertyName("phobia")] public Phobia Phobia { get; set; } = Phobia.None;
    [JsonPropertyName("phobiaModel")] public Model? PhobiaModel { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(2);
        if (Hitbox is not null) bits.SetBit(1);
        if (Light is not null) bits.SetBit(2);
        if (AssetId is not null) bits.SetBit(4);
        if (Path is not null) bits.SetBit(8);
        if (Texture is not null) bits.SetBit(16);
        if (GradientSet is not null) bits.SetBit(32);
        if (GradientId is not null) bits.SetBit(64);
        if (Camera is not null) bits.SetBit(128);
        if (AnimationSets is not null) bits.SetBit(256);
        if (Attachments is not null) bits.SetBit(512);
        if (Particles is not null) bits.SetBit(1024);
        if (Trails is not null) bits.SetBit(2048);
        if (DetailBoxes is not null) bits.SetBit(4096);
        if (PhobiaModel is not null) bits.SetBit(8192);

        writer.WriteBits(bits);

        // Fixed Block
        writer.WriteFloat32(Scale);
        writer.WriteFloat32(EyeHeight);
        writer.WriteFloat32(CrouchOffset);
        if (Hitbox is not null)
        {
            Hitbox.Serialize(writer);
        }
        else
        {
            writer.WriteInt64(0);
            writer.WriteInt64(0);
            writer.WriteInt64(0);
        }
        if (Light is not null) Light.Value.Serialize(writer);
        else writer.WriteInt32(0);
        writer.WriteEnum(Phobia);

        // Reserve Offsets
        var offsets = new int[12];
        for (int i = 0; i < 12; i++) offsets[i] = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // Variable Block
        if (AssetId is not null)
        {
            writer.WriteOffsetAt(offsets[0], writer.Position - varBlockStart);
            writer.WriteVarUtf8String(AssetId, 4096000);
        }
        else writer.WriteOffsetAt(offsets[0], -1);

        if (Path is not null)
        {
            writer.WriteOffsetAt(offsets[1], writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Path, 4096000);
        }
        else writer.WriteOffsetAt(offsets[1], -1);

        if (Texture is not null)
        {
            writer.WriteOffsetAt(offsets[2], writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Texture, 4096000);
        }
        else writer.WriteOffsetAt(offsets[2], -1);

        if (GradientSet is not null)
        {
            writer.WriteOffsetAt(offsets[3], writer.Position - varBlockStart);
            writer.WriteVarUtf8String(GradientSet, 4096000);
        }
        else writer.WriteOffsetAt(offsets[3], -1);

        if (GradientId is not null)
        {
            writer.WriteOffsetAt(offsets[4], writer.Position - varBlockStart);
            writer.WriteVarUtf8String(GradientId, 4096000);
        }
        else writer.WriteOffsetAt(offsets[4], -1);

        if (Camera is not null)
        {
            writer.WriteOffsetAt(offsets[5], writer.Position - varBlockStart);
            Camera.Serialize(writer);
        }
        else writer.WriteOffsetAt(offsets[5], -1);

        if (AnimationSets is not null)
        {
            writer.WriteOffsetAt(offsets[6], writer.Position - varBlockStart);
            writer.WriteVarInt(AnimationSets.Count);
            foreach (var (k, v) in AnimationSets)
            {
                writer.WriteVarUtf8String(k, 4096000);
                v.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(offsets[6], -1);

        if (Attachments is not null)
        {
            writer.WriteOffsetAt(offsets[7], writer.Position - varBlockStart);
            writer.WriteVarInt(Attachments.Length);
            foreach (var item in Attachments) item.Serialize(writer);
        }
        else writer.WriteOffsetAt(offsets[7], -1);

        if (Particles is not null)
        {
            writer.WriteOffsetAt(offsets[8], writer.Position - varBlockStart);
            writer.WriteVarInt(Particles.Length);
            foreach (var item in Particles) item.Serialize(writer);
        }
        else writer.WriteOffsetAt(offsets[8], -1);

        if (Trails is not null)
        {
            writer.WriteOffsetAt(offsets[9], writer.Position - varBlockStart);
            writer.WriteVarInt(Trails.Length);
            foreach (var item in Trails) item.Serialize(writer);
        }
        else writer.WriteOffsetAt(offsets[9], -1);

        if (DetailBoxes is not null)
        {
            writer.WriteOffsetAt(offsets[10], writer.Position - varBlockStart);
            writer.WriteVarInt(DetailBoxes.Count);
            foreach (var (k, v) in DetailBoxes)
            {
                writer.WriteVarUtf8String(k, 4096000);
                writer.WriteVarInt(v.Length);
                foreach (var item in v) item.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(offsets[10], -1);

        if (PhobiaModel is not null)
        {
            writer.WriteOffsetAt(offsets[11], writer.Position - varBlockStart);
            PhobiaModel.Serialize(writer);
        }
        else writer.WriteOffsetAt(offsets[11], -1);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        // Fixed Block
        Scale = reader.ReadFloat32();
        EyeHeight = reader.ReadFloat32();
        CrouchOffset = reader.ReadFloat32();
        if (bits.IsSet(1)) Hitbox = reader.ReadObject<Hitbox>();
        else reader.ReadBytes(24);
        if (bits.IsSet(2)) Light = reader.ReadObject<ColorLight>();
        else reader.ReadInt32();
        Phobia = reader.ReadEnum<Phobia>();

        // Read Offsets
        var offsets = reader.ReadOffsets(12);

        // Variable Block
        if (bits.IsSet(4)) AssetId = reader.ReadVarUtf8StringAt(offsets[0]);
        if (bits.IsSet(8)) Path = reader.ReadVarUtf8StringAt(offsets[1]);
        if (bits.IsSet(16)) Texture = reader.ReadVarUtf8StringAt(offsets[2]);
        if (bits.IsSet(32)) GradientSet = reader.ReadVarUtf8StringAt(offsets[3]);
        if (bits.IsSet(64)) GradientId = reader.ReadVarUtf8StringAt(offsets[4]);
        if (bits.IsSet(128)) Camera = reader.ReadObjectAt<CameraSettings>(offsets[5]);
        if (bits.IsSet(256))
            AnimationSets =
                reader.ReadDictionaryAt(offsets[6], r => r.ReadUtf8String(), r => r.ReadObject<AnimationSet>());
        if (bits.IsSet(512)) Attachments = reader.ReadObjectArrayAt<ModelAttachment>(offsets[7]);
        if (bits.IsSet(1024)) Particles = reader.ReadObjectArrayAt<ModelParticle>(offsets[8]);
        if (bits.IsSet(2048)) Trails = reader.ReadObjectArrayAt<ModelTrail>(offsets[9]);
        if (bits.IsSet(4096))
            DetailBoxes = reader.ReadDictionaryAt(offsets[10], r => r.ReadUtf8String(),
                r => r.ReadObjectArray<DetailBox>());
        if (bits.IsSet(8192)) PhobiaModel = reader.ReadObjectAt<Model>(offsets[11]);
    }
}