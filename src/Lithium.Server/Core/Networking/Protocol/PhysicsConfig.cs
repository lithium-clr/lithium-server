using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class PhysicsConfig : INetworkSerializable
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<PhysicsType>))]
    public PhysicsType Type { get; set; } = PhysicsType.Standard;
    
    [JsonPropertyName("density")] public double Density { get; set; }
    [JsonPropertyName("gravity")] public double Gravity { get; set; }
    [JsonPropertyName("bounciness")] public double Bounciness { get; set; }
    [JsonPropertyName("bounceCount")] public int BounceCount { get; set; }
    [JsonPropertyName("bounceLimit")] public double BounceLimit { get; set; }
    [JsonPropertyName("sticksVertically")] public bool SticksVertically { get; set; }
    [JsonPropertyName("computeYaw")] public bool ComputeYaw { get; set; }
    [JsonPropertyName("computePitch")] public bool ComputePitch { get; set; }
    
    [JsonPropertyName("rotationMode")]
    [JsonConverter(typeof(JsonStringEnumConverter<RotationMode>))]
    public RotationMode RotationMode { get; set; } = RotationMode.None;
    
    [JsonPropertyName("moveOutOfSolidSpeed")] public double MoveOutOfSolidSpeed { get; set; }
    [JsonPropertyName("terminalVelocityAir")] public double TerminalVelocityAir { get; set; }
    [JsonPropertyName("densityAir")] public double DensityAir { get; set; }
    [JsonPropertyName("terminalVelocityWater")] public double TerminalVelocityWater { get; set; }
    [JsonPropertyName("densityWater")] public double DensityWater { get; set; }
    [JsonPropertyName("hitWaterImpulseLoss")] public double HitWaterImpulseLoss { get; set; }
    [JsonPropertyName("rotationForce")] public double RotationForce { get; set; }
    [JsonPropertyName("speedRotationFactor")] public float SpeedRotationFactor { get; set; }
    [JsonPropertyName("swimmingDampingFactor")] public double SwimmingDampingFactor { get; set; }
    [JsonPropertyName("allowRolling")] public bool AllowRolling { get; set; }
    [JsonPropertyName("rollingFrictionFactor")] public double RollingFrictionFactor { get; set; }
    [JsonPropertyName("rollingSpeed")] public float RollingSpeed { get; set; }

    public void Serialize(PacketWriter writer)
    {
        if (Enum.IsDefined(Type))
        {
            writer.WriteEnum(Type);
        }
        else
        {
            writer.WriteEnum(PhysicsType.Standard);
        }

        writer.WriteFloat64(Density);
        writer.WriteFloat64(Gravity);
        writer.WriteFloat64(Bounciness);
        writer.WriteInt32(BounceCount);
        writer.WriteFloat64(BounceLimit);
        writer.WriteBoolean(SticksVertically);
        writer.WriteBoolean(ComputeYaw);
        writer.WriteBoolean(ComputePitch);

        if (Enum.IsDefined(RotationMode))
        {
            writer.WriteEnum(RotationMode);
        }
        else
        {
            writer.WriteEnum(RotationMode.None);
        }

        writer.WriteFloat64(MoveOutOfSolidSpeed);
        writer.WriteFloat64(TerminalVelocityAir);
        writer.WriteFloat64(DensityAir);
        writer.WriteFloat64(TerminalVelocityWater);
        writer.WriteFloat64(DensityWater);
        writer.WriteFloat64(HitWaterImpulseLoss);
        writer.WriteFloat64(RotationForce);
        writer.WriteFloat32(SpeedRotationFactor);
        writer.WriteFloat64(SwimmingDampingFactor);
        writer.WriteBoolean(AllowRolling);
        writer.WriteFloat64(RollingFrictionFactor);
        writer.WriteFloat32(RollingSpeed);
    }

    public void Deserialize(PacketReader reader)
    {
        Type = reader.ReadEnum<PhysicsType>();
        Density = reader.ReadFloat64();
        Gravity = reader.ReadFloat64();
        Bounciness = reader.ReadFloat64();
        BounceCount = reader.ReadInt32();
        BounceLimit = reader.ReadFloat64();
        SticksVertically = reader.ReadBoolean();
        ComputeYaw = reader.ReadBoolean();
        ComputePitch = reader.ReadBoolean();
        RotationMode = reader.ReadEnum<RotationMode>();
        MoveOutOfSolidSpeed = reader.ReadFloat64();
        TerminalVelocityAir = reader.ReadFloat64();
        DensityAir = reader.ReadFloat64();
        TerminalVelocityWater = reader.ReadFloat64();
        DensityWater = reader.ReadFloat64();
        HitWaterImpulseLoss = reader.ReadFloat64();
        RotationForce = reader.ReadFloat64();
        SpeedRotationFactor = reader.ReadFloat32();
        SwimmingDampingFactor = reader.ReadFloat64();
        AllowRolling = reader.ReadBoolean();
        RollingFrictionFactor = reader.ReadFloat64();
        RollingSpeed = reader.ReadFloat32();
    }
}