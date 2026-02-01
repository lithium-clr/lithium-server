using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

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
    [JsonPropertyName("animationSets")] public Dictionary<string, AnimationSet>? AnimationSets { get; set; }
    [JsonPropertyName("attachments")] public ModelAttachment[]? Attachments { get; set; }
    [JsonPropertyName("hitbox")] public Hitbox? Hitbox { get; set; }
    [JsonPropertyName("particles")] public ModelParticle[]? Particles { get; set; }
    [JsonPropertyName("trails")] public ModelTrail[]? Trails { get; set; }
    [JsonPropertyName("light")] public ColorLight? Light { get; set; }
    [JsonPropertyName("detailBoxes")] public Dictionary<string, DetailBox[]>? DetailBoxes { get; set; }

    [JsonPropertyName("phobia")]
    [JsonConverter(typeof(JsonStringEnumConverter<Phobia>))]
    public Phobia Phobia { get; set; } = Phobia.None;

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

        // 1. BITS (2 bytes)
        // 1. BITS (2 bytes)
        writer.WriteBits(bits);

        // 2. FIXED BLOCK - 41 bytes
        writer.WriteFloat32(Scale);
        writer.WriteFloat32(EyeHeight);
        writer.WriteFloat32(CrouchOffset);

        if (Hitbox is not null)
            Hitbox.Serialize(writer);
        else
            writer.WriteZero(24);

        if (Light is not null)
            Light.Serialize(writer);
        else
            writer.WriteZero(4);

        writer.WriteEnum(Phobia);

        // 3. OFFSETS (12 offsets × 4 bytes = 48 bytes)
        var assetIdOffsetSlot = writer.ReserveOffset();
        var pathOffsetSlot = writer.ReserveOffset();
        var textureOffsetSlot = writer.ReserveOffset();
        var gradientSetOffsetSlot = writer.ReserveOffset();
        var gradientIdOffsetSlot = writer.ReserveOffset();
        var cameraOffsetSlot = writer.ReserveOffset();
        var animationSetsOffsetSlot = writer.ReserveOffset();
        var attachmentsOffsetSlot = writer.ReserveOffset();
        var particlesOffsetSlot = writer.ReserveOffset();
        var trailsOffsetSlot = writer.ReserveOffset();
        var detailBoxesOffsetSlot = writer.ReserveOffset();
        var phobiaModelOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // 4. VARIABLE BLOCK - AssetId
        if (AssetId is not null)
        {
            writer.WriteOffsetAt(assetIdOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(AssetId, 4096000);
        }
        else writer.WriteOffsetAt(assetIdOffsetSlot, -1);

        // 5. VARIABLE BLOCK - Path
        // 5. VARIABLE BLOCK - Path
        if (Path is not null)
        {
            writer.WriteOffsetAt(pathOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Path, 4096000);
        }
        else writer.WriteOffsetAt(pathOffsetSlot, -1);

        // 6. VARIABLE BLOCK - Texture
        // 6. VARIABLE BLOCK - Texture
        if (Texture is not null)
        {
            writer.WriteOffsetAt(textureOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Texture, 4096000);
        }
        else writer.WriteOffsetAt(textureOffsetSlot, -1);

        // 7. VARIABLE BLOCK - GradientSet
        // 7. VARIABLE BLOCK - GradientSet
        if (GradientSet is not null)
        {
            writer.WriteOffsetAt(gradientSetOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(GradientSet, 4096000);
        }
        else writer.WriteOffsetAt(gradientSetOffsetSlot, -1);

        // 8. VARIABLE BLOCK - GradientId
        // 8. VARIABLE BLOCK - GradientId
        if (GradientId is not null)
        {
            writer.WriteOffsetAt(gradientIdOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(GradientId, 4096000);
        }
        else writer.WriteOffsetAt(gradientIdOffsetSlot, -1);

        // 9. VARIABLE BLOCK - Camera
        // 9. VARIABLE BLOCK - Camera
        if (Camera is not null)
        {
            writer.WriteOffsetAt(cameraOffsetSlot, writer.Position - varBlockStart);
            Camera.Serialize(writer);
        }
        else writer.WriteOffsetAt(cameraOffsetSlot, -1);

        // 10. VARIABLE BLOCK - AnimationSets (Dictionary<string, AnimationSet>)
        // 10. VARIABLE BLOCK - AnimationSets (Dictionary<string, AnimationSet>)
        if (AnimationSets is not null)
        {
            writer.WriteOffsetAt(animationSetsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(AnimationSets.Count);
            foreach (var kvp in AnimationSets)
            {
                writer.WriteVarUtf8String(kvp.Key, 4096000);
                kvp.Value.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(animationSetsOffsetSlot, -1);

        // 11. VARIABLE BLOCK - Attachments
        // 11. VARIABLE BLOCK - Attachments
        if (Attachments is not null)
        {
            writer.WriteOffsetAt(attachmentsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Attachments.Length);
            foreach (var item in Attachments)
                item.Serialize(writer);
        }
        else writer.WriteOffsetAt(attachmentsOffsetSlot, -1);

        // 12. VARIABLE BLOCK - Particles
        // 12. VARIABLE BLOCK - Particles
        if (Particles is not null)
        {
            writer.WriteOffsetAt(particlesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Particles.Length);
            foreach (var item in Particles)
                item.Serialize(writer);
        }
        else writer.WriteOffsetAt(particlesOffsetSlot, -1);

        // 13. VARIABLE BLOCK - Trails
        // 13. VARIABLE BLOCK - Trails
        if (Trails is not null)
        {
            writer.WriteOffsetAt(trailsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Trails.Length);
            foreach (var item in Trails)
                item.Serialize(writer);
        }
        else writer.WriteOffsetAt(trailsOffsetSlot, -1);

        // 14. VARIABLE BLOCK - DetailBoxes (Dictionary<string, DetailBox[]>)
        // 14. VARIABLE BLOCK - DetailBoxes (Dictionary<string, DetailBox[]>)
        if (DetailBoxes is not null)
        {
            writer.WriteOffsetAt(detailBoxesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(DetailBoxes.Count);
            foreach (var kvp in DetailBoxes)
            {
                writer.WriteVarUtf8String(kvp.Key, 4096000);
                writer.WriteVarInt(kvp.Value.Length);
                foreach (var item in kvp.Value)
                    item.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(detailBoxesOffsetSlot, -1);

        // 15. VARIABLE BLOCK - PhobiaModel (RÉCURSIF)
        // 15. VARIABLE BLOCK - PhobiaModel (RÉCURSIF)
        if (PhobiaModel is not null)
        {
            writer.WriteOffsetAt(phobiaModelOffsetSlot, writer.Position - varBlockStart);
            PhobiaModel.Serialize(writer);
        }
        else writer.WriteOffsetAt(phobiaModelOffsetSlot, -1);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        // FIXED BLOCK
        Scale = reader.ReadFloat32();
        EyeHeight = reader.ReadFloat32();
        CrouchOffset = reader.ReadFloat32();

        if (bits.IsSet(1))
            Hitbox = reader.ReadObject<Hitbox>();

        if (bits.IsSet(2))
            Light = reader.ReadObject<ColorLight>();

        Phobia = reader.ReadEnum<Phobia>();

        var offsets = reader.ReadOffsets(12);

        if (bits.IsSet(4))
            AssetId = reader.ReadVarUtf8StringAt(offsets[0]);

        if (bits.IsSet(8))
            Path = reader.ReadVarUtf8StringAt(offsets[1]);

        if (bits.IsSet(16))
            Texture = reader.ReadVarUtf8StringAt(offsets[2]);

        if (bits.IsSet(32))
            GradientSet = reader.ReadVarUtf8StringAt(offsets[3]);

        if (bits.IsSet(64))
            GradientId = reader.ReadVarUtf8StringAt(offsets[4]);

        if (bits.IsSet(128))
            Camera = reader.ReadObjectAt<CameraSettings>(offsets[5]);

        if (bits.IsSet(256))
        {
            AnimationSets = reader.ReadDictionaryAt(
                offsets[6],
                r => r.ReadUtf8String(),
                r => r.ReadObject<AnimationSet>()
            );
        }

        if (bits.IsSet(512))
        {
            Attachments = reader.ReadArrayAt(
                offsets[7],
                r => r.ReadObject<ModelAttachment>()
            );
        }

        if (bits.IsSet(1024))
        {
            Particles = reader.ReadArrayAt(
                offsets[8],
                r => r.ReadObject<ModelParticle>()
            );
        }

        if (bits.IsSet(2048))
        {
            Trails = reader.ReadArrayAt(
                offsets[9],
                r => r.ReadObject<ModelTrail>()
            );
        }

        if (bits.IsSet(4096))
        {
            // Dictionary<string, DetailBox[]> - nécessite une lecture spéciale
            var offset = offsets[10];
            if (offset != -1)
            {
                var savedPos = reader.GetPosition();
                reader.SeekTo(reader.VariableBlockStart + offset);

                var count = reader.ReadVarInt32();
                DetailBoxes = new Dictionary<string, DetailBox[]>(count);

                for (var i = 0; i < count; i++)
                {
                    var key = reader.ReadUtf8String();
                    var arrayLength = reader.ReadVarInt32();
                    var array = new DetailBox[arrayLength];

                    for (var j = 0; j < arrayLength; j++)
                        array[j] = reader.ReadObject<DetailBox>();

                    DetailBoxes[key] = array;
                }

                reader.SeekTo(savedPos);
            }
        }

        if (bits.IsSet(8192))
            PhobiaModel = reader.ReadObjectAt<Model>(offsets[11]); // RÉCURSIF
    }
}