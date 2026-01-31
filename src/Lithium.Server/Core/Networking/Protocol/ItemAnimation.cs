using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ItemAnimation : INetworkSerializable
{
    [JsonPropertyName("thirdPerson")] public string? ThirdPerson { get; set; }

    [JsonPropertyName("thirdPersonMoving")]
    public string? ThirdPersonMoving { get; set; }

    [JsonPropertyName("thirdPersonFace")] public string? ThirdPersonFace { get; set; }
    [JsonPropertyName("firstPerson")] public string? FirstPerson { get; set; }

    [JsonPropertyName("firstPersonOverride")]
    public string? FirstPersonOverride { get; set; }

    [JsonPropertyName("keepPreviousFirstPersonAnimation")]
    public bool KeepPreviousFirstPersonAnimation { get; set; }

    [JsonPropertyName("speed")] public float Speed { get; set; }
    [JsonPropertyName("blendingDuration")] public float BlendingDuration { get; set; } = 0.2F;
    [JsonPropertyName("looping")] public bool Looping { get; set; }
    [JsonPropertyName("clipsGeometry")] public bool ClipsGeometry { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (ThirdPerson is not null)
            bits.SetBit(1);

        if (ThirdPersonMoving is not null)
            bits.SetBit(2);

        if (ThirdPersonFace is not null)
            bits.SetBit(4);

        if (FirstPerson is not null)
            bits.SetBit(8);

        if (FirstPersonOverride is not null)
            bits.SetBit(16);

        writer.WriteBits(bits);

        writer.WriteBoolean(KeepPreviousFirstPersonAnimation);
        writer.WriteFloat32(Speed);
        writer.WriteFloat32(BlendingDuration);
        writer.WriteBoolean(Looping);
        writer.WriteBoolean(ClipsGeometry);

        var thirdPersonOffsetSlot = writer.ReserveOffset();
        var thirdPersonMovingOffsetSlot = writer.ReserveOffset();
        var thirdPersonFaceOffsetSlot = writer.ReserveOffset();
        var firstPersonOffsetSlot = writer.ReserveOffset();
        var firstPersonOverrideOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (ThirdPerson is not null)
        {
            writer.WriteOffsetAt(thirdPersonOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(ThirdPerson, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(thirdPersonOffsetSlot, -1);
        }

        if (ThirdPersonMoving is not null)
        {
            writer.WriteOffsetAt(thirdPersonMovingOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(ThirdPersonMoving, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(thirdPersonMovingOffsetSlot, -1);
        }

        if (ThirdPersonFace is not null)
        {
            writer.WriteOffsetAt(thirdPersonFaceOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(ThirdPersonFace, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(thirdPersonFaceOffsetSlot, -1);
        }

        if (FirstPerson is not null)
        {
            writer.WriteOffsetAt(firstPersonOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(FirstPerson, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(firstPersonOffsetSlot, -1);
        }

        if (FirstPersonOverride is not null)
        {
            writer.WriteOffsetAt(firstPersonOverrideOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(FirstPersonOverride, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(firstPersonOverrideOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        KeepPreviousFirstPersonAnimation = reader.ReadBoolean();
        Speed = reader.ReadFloat32();
        BlendingDuration = reader.ReadFloat32();
        Looping = reader.ReadBoolean();
        ClipsGeometry = reader.ReadBoolean();

        var offsets = reader.ReadOffsets(5);

        if (bits.IsSet(1))
            ThirdPerson = reader.ReadVarUtf8StringAt(offsets[0]);

        if (bits.IsSet(2))
            ThirdPersonMoving = reader.ReadVarUtf8StringAt(offsets[1]);

        if (bits.IsSet(4))
            ThirdPersonFace = reader.ReadVarUtf8StringAt(offsets[2]);

        if (bits.IsSet(8))
            FirstPerson = reader.ReadVarUtf8StringAt(offsets[3]);

        if (bits.IsSet(16))
            FirstPersonOverride = reader.ReadVarUtf8StringAt(offsets[4]);
    }
}