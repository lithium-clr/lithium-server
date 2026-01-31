using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ItemPlayerAnimations : INetworkSerializable
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("animations")] public Dictionary<string, ItemAnimation>? Animations { get; set; }
    [JsonPropertyName("wiggleWeights")] public WiggleWeights? WiggleWeights { get; set; }
    [JsonPropertyName("cameraSettings")] public CameraSettings? CameraSettings { get; set; }
    [JsonPropertyName("pullbackConfig")] public ItemPullbackConfiguration? PullbackConfiguration { get; set; }

    [JsonPropertyName("useFirstPersonOverride")]
    public bool UseFirstPersonOverride { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (WiggleWeights is not null)
            bits.SetBit(1);

        if (PullbackConfiguration is not null)
            bits.SetBit(2);

        if (Id is not null)
            bits.SetBit(4);

        if (Animations is not null)
            bits.SetBit(8);

        if (CameraSettings is not null)
            bits.SetBit(16);

        writer.WriteBits(bits);

        if (WiggleWeights is not null)
        {
            WiggleWeights.Serialize(writer);
        }
        else
        {
            writer.WriteZero(40);
        }

        if (PullbackConfiguration is not null)
        {
            PullbackConfiguration.Serialize(writer);
        }
        else
        {
            writer.WriteZero(49);
        }

        writer.WriteBoolean(UseFirstPersonOverride);

        var idOffsetSlot = writer.ReserveOffset();
        var animationsOffsetSlot = writer.ReserveOffset();
        var cameraSettingsOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Id is not null)
        {
            writer.WriteOffsetAt(idOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Id, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(idOffsetSlot, -1);
        }

        if (Animations is not null)
        {
            writer.WriteOffsetAt(animationsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Animations.Count);

            foreach (var entry in Animations)
            {
                writer.WriteVarUtf8String(entry.Key, 4096000);
                entry.Value.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(animationsOffsetSlot, -1);
        }

        if (CameraSettings is not null)
        {
            writer.WriteOffsetAt(cameraSettingsOffsetSlot, writer.Position - varBlockStart);
            CameraSettings.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(cameraSettingsOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
            WiggleWeights = reader.ReadObject<WiggleWeights>();

        if (bits.IsSet(2))
            PullbackConfiguration = reader.ReadObject<ItemPullbackConfiguration>();

        UseFirstPersonOverride = reader.ReadBoolean();

        var offsets = reader.ReadOffsets(3);

        if (bits.IsSet(4))
            Id = reader.ReadVarUtf8StringAt(offsets[0]);

        if (bits.IsSet(8))
            Animations = reader.ReadAnimationsDictionary<ItemAnimation>(offsets[1]);

        if (bits.IsSet(16))
            CameraSettings = reader.ReadObjectAt<CameraSettings>(offsets[2]);
    }
}