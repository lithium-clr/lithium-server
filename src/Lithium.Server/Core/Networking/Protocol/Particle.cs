using System.Text.Json.Serialization;
using Lithium.Server.Core.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 133,
    VariableFieldCount = 2,
    VariableBlockStart = 141,
    MaxSize = 270336151
)]
public sealed class Particle : INetworkSerializable
{
    [JsonPropertyName("texturePath")]
    public string? TexturePath { get; set; }

    [JsonPropertyName("frameSize")]
    public Vector2Int? FrameSize { get; set; }

    [JsonPropertyName("uvOption")]
    [JsonConverter(typeof(JsonStringEnumConverter<ParticleUVOption>))]
    public ParticleUVOption UvOption { get; set; } = ParticleUVOption.None;

    [JsonPropertyName("scaleRatioConstraint")]
    [JsonConverter(typeof(JsonStringEnumConverter<ParticleScaleRatioConstraint>))]
    public ParticleScaleRatioConstraint ScaleRatioConstraint { get; set; } = ParticleScaleRatioConstraint.OneToOne;

    [JsonPropertyName("softParticles")]
    [JsonConverter(typeof(JsonStringEnumConverter<SoftParticle>))]
    public SoftParticle SoftParticles { get; set; } = SoftParticle.Enable;

    [JsonPropertyName("softParticlesFadeFactor")]
    public float SoftParticlesFadeFactor { get; set; }

    [JsonPropertyName("useSpriteBlending")]
    public bool UseSpriteBlending { get; set; }

    [JsonPropertyName("initialAnimationFrame")]
    public ParticleAnimationFrame? InitialAnimationFrame { get; set; }

    [JsonPropertyName("collisionAnimationFrame")]
    public ParticleAnimationFrame? CollisionAnimationFrame { get; set; }

    [JsonPropertyName("animationFrames")]
    [JsonConverter(typeof(IntKeyDictionaryConverter<ParticleAnimationFrame>))]
    public Dictionary<int, ParticleAnimationFrame>? AnimationFrames { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (FrameSize is not null) bits.SetBit(1);
        if (InitialAnimationFrame is not null) bits.SetBit(2);
        if (CollisionAnimationFrame is not null) bits.SetBit(4);
        if (TexturePath is not null) bits.SetBit(8);
        if (AnimationFrames is not null) bits.SetBit(16);

        writer.WriteBits(bits);

        if (FrameSize is not null) FrameSize.Serialize(writer);
        else writer.WriteZero(8);

        writer.WriteEnum(UvOption);
        writer.WriteEnum(ScaleRatioConstraint);
        writer.WriteEnum(SoftParticles);
        writer.WriteFloat32(SoftParticlesFadeFactor);
        writer.WriteBoolean(UseSpriteBlending);

        if (InitialAnimationFrame is not null) InitialAnimationFrame.Serialize(writer);
        else writer.WriteZero(58);

        if (CollisionAnimationFrame is not null) CollisionAnimationFrame.Serialize(writer);
        else writer.WriteZero(58);

        var texturePathOffsetSlot = writer.ReserveOffset();
        var animationFramesOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (TexturePath is not null)
        {
            writer.WriteOffsetAt(texturePathOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(TexturePath, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(texturePathOffsetSlot, -1);
        }

        if (AnimationFrames is not null)
        {
            writer.WriteOffsetAt(animationFramesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(AnimationFrames.Count);
            foreach (var (key, value) in AnimationFrames)
            {
                writer.WriteInt32(key);
                value.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(animationFramesOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1)) FrameSize = reader.ReadObject<Vector2Int>();
        else reader.SeekTo(reader.GetPosition() + 8);

        UvOption = reader.ReadEnum<ParticleUVOption>();
        ScaleRatioConstraint = reader.ReadEnum<ParticleScaleRatioConstraint>();
        SoftParticles = reader.ReadEnum<SoftParticle>();
        SoftParticlesFadeFactor = reader.ReadFloat32();
        UseSpriteBlending = reader.ReadBoolean();

        if (bits.IsSet(2)) InitialAnimationFrame = reader.ReadObject<ParticleAnimationFrame>();
        else reader.SeekTo(reader.GetPosition() + 58);

        if (bits.IsSet(4)) CollisionAnimationFrame = reader.ReadObject<ParticleAnimationFrame>();
        else reader.SeekTo(reader.GetPosition() + 58);

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(8))
        {
            TexturePath = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(16))
        {
            AnimationFrames = reader.ReadDictionaryAt(
                offsets[1],
                r => r.ReadInt32(),
                r =>
                {
                    var frame = new ParticleAnimationFrame();
                    frame.Deserialize(r);
                    return frame;
                }
            );
        }
    }
}