using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public class ApplyForceInteraction : SimpleInteraction
{
    public override int TypeId => 26;
    
    [JsonPropertyName("velocityConfig")] public VelocityConfig? VelocityConfig { get; set; }

    [JsonPropertyName("changeVelocityType")]
    public ChangeVelocityType ChangeVelocityType { get; set; } = ChangeVelocityType.Add;

    [JsonPropertyName("forces")] public AppliedForce[]? Forces { get; set; }
    [JsonPropertyName("duration")] public float Duration { get; set; }
    [JsonPropertyName("verticalClamp")] public FloatRange? VerticalClamp { get; set; }
    [JsonPropertyName("waitForGround")] public bool WaitForGround { get; set; }
    [JsonPropertyName("waitForCollision")] public bool WaitForCollision { get; set; }
    [JsonPropertyName("groundCheckDelay")] public float GroundCheckDelay { get; set; }

    [JsonPropertyName("collisionCheckDelay")]
    public float CollisionCheckDelay { get; set; }

    [JsonPropertyName("groundNext")] public int GroundNext { get; set; }
    [JsonPropertyName("collisionNext")] public int CollisionNext { get; set; }
    [JsonPropertyName("raycastDistance")] public float RaycastDistance { get; set; }

    [JsonPropertyName("raycastHeightOffset")]
    public float RaycastHeightOffset { get; set; }

    [JsonPropertyName("raycastMode")] public RaycastMode RaycastMode { get; set; } = RaycastMode.FollowMotion;

    public override void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (VelocityConfig is not null) bits.SetBit(1);
        if (VerticalClamp is not null) bits.SetBit(2);
        if (Effects is not null) bits.SetBit(4);
        if (Settings is not null) bits.SetBit(8);
        if (Rules is not null) bits.SetBit(16);
        if (Tags is not null) bits.SetBit(32);
        if (Camera is not null) bits.SetBit(64);
        if (Forces is not null) bits.SetBit(128);
        writer.WriteBits(bits);

        writer.WriteEnum(WaitForDataFrom);
        writer.WriteFloat32(HorizontalSpeedMultiplier);
        writer.WriteFloat32(RunTime);
        writer.WriteBoolean(CancelOnItemChange);
        writer.WriteInt32(Next);
        writer.WriteInt32(Failed);

        if (VelocityConfig is not null) VelocityConfig.Serialize(writer);
        else writer.WriteZero(21);
        writer.WriteEnum(ChangeVelocityType);
        writer.WriteFloat32(Duration);
        if (VerticalClamp is not null) VerticalClamp.Serialize(writer);
        else writer.WriteZero(8);

        writer.WriteBoolean(WaitForGround);
        writer.WriteBoolean(WaitForCollision);
        writer.WriteFloat32(GroundCheckDelay);
        writer.WriteFloat32(CollisionCheckDelay);
        writer.WriteInt32(GroundNext);
        writer.WriteInt32(CollisionNext);
        writer.WriteFloat32(RaycastDistance);
        writer.WriteFloat32(RaycastHeightOffset);
        writer.WriteEnum(RaycastMode);

        var effectsOffsetSlot = writer.ReserveOffset();
        var settingsOffsetSlot = writer.ReserveOffset();
        var rulesOffsetSlot = writer.ReserveOffset();
        var tagsOffsetSlot = writer.ReserveOffset();
        var cameraOffsetSlot = writer.ReserveOffset();
        var forcesOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Effects is not null)
        {
            writer.WriteOffsetAt(effectsOffsetSlot, writer.Position - varBlockStart);
            Effects.Serialize(writer);
        }
        else writer.WriteOffsetAt(effectsOffsetSlot, -1);

        if (Settings is not null)
        {
            writer.WriteOffsetAt(settingsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Settings.Count);
            foreach (var (key, value) in Settings)
            {
                writer.WriteEnum(key);
                value.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(settingsOffsetSlot, -1);

        if (Rules is not null)
        {
            writer.WriteOffsetAt(rulesOffsetSlot, writer.Position - varBlockStart);
            Rules.Serialize(writer);
        }
        else writer.WriteOffsetAt(rulesOffsetSlot, -1);

        if (Tags is not null)
        {
            writer.WriteOffsetAt(tagsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Tags.Length);
            foreach (var tag in Tags) writer.WriteInt32(tag);
        }
        else writer.WriteOffsetAt(tagsOffsetSlot, -1);

        if (Camera is not null)
        {
            writer.WriteOffsetAt(cameraOffsetSlot, writer.Position - varBlockStart);
            Camera.Serialize(writer);
        }
        else writer.WriteOffsetAt(cameraOffsetSlot, -1);

        if (Forces is not null)
        {
            writer.WriteOffsetAt(forcesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Forces.Length);
            foreach (var f in Forces) f.Serialize(writer);
        }
        else writer.WriteOffsetAt(forcesOffsetSlot, -1);
    }

    public override void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        WaitForDataFrom = reader.ReadEnum<WaitForDataFrom>();
        HorizontalSpeedMultiplier = reader.ReadFloat32();
        RunTime = reader.ReadFloat32();
        CancelOnItemChange = reader.ReadBoolean();
        Next = reader.ReadInt32();
        Failed = reader.ReadInt32();

        if (bits.IsSet(1))
        {
            VelocityConfig = new VelocityConfig();
            VelocityConfig.Deserialize(reader);
        }
        else reader.SeekTo(reader.GetPosition() + 21);

        ChangeVelocityType = reader.ReadEnum<ChangeVelocityType>();
        Duration = reader.ReadFloat32();

        if (bits.IsSet(2))
        {
            VerticalClamp = new FloatRange();
            VerticalClamp.Deserialize(reader);
        }
        else reader.SeekTo(reader.GetPosition() + 8);

        WaitForGround = reader.ReadBoolean();
        WaitForCollision = reader.ReadBoolean();
        GroundCheckDelay = reader.ReadFloat32();
        CollisionCheckDelay = reader.ReadFloat32();
        GroundNext = reader.ReadInt32();
        CollisionNext = reader.ReadInt32();
        RaycastDistance = reader.ReadFloat32();
        RaycastHeightOffset = reader.ReadFloat32();
        RaycastMode = reader.ReadEnum<RaycastMode>();

        var offsets = reader.ReadOffsets(6);

        if (bits.IsSet(4)) Effects = reader.ReadObjectAt<InteractionEffects>(offsets[0]);
        if (bits.IsSet(8))
            Settings = reader.ReadDictionaryAt(offsets[1], r => r.ReadEnum<GameMode>(), r =>
            {
                var s = new InteractionSettings();
                s.Deserialize(r);
                return s;
            });
        if (bits.IsSet(16)) Rules = reader.ReadObjectAt<InteractionRules>(offsets[2]);
        if (bits.IsSet(32)) Tags = reader.ReadArrayAt(offsets[3], r => r.ReadInt32());
        if (bits.IsSet(64)) Camera = reader.ReadObjectAt<InteractionCameraSettings>(offsets[4]);
        if (bits.IsSet(128))
            Forces = reader.ReadArrayAt(offsets[5], r =>
            {
                var f = new AppliedForce();
                f.Deserialize(r);
                return f;
            });
    }

    public override int ComputeSize() => 104;
}