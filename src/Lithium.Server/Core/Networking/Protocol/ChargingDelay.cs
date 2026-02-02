using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 0,
    FixedBlockSize = 20,
    VariableFieldCount = 0,
    VariableBlockStart = 20,
    MaxSize = 20
)]
public sealed class ChargingDelay : INetworkSerializable
{
    [JsonPropertyName("minDelay")]      public float MinDelay      { get; set; }
    [JsonPropertyName("maxDelay")]      public float MaxDelay      { get; set; }
    [JsonPropertyName("maxTotalDelay")] public float MaxTotalDelay { get; set; }
    [JsonPropertyName("minHealth")]     public float MinHealth     { get; set; }
    [JsonPropertyName("maxHealth")]     public float MaxHealth     { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(MinDelay);
        writer.WriteFloat32(MaxDelay);
        writer.WriteFloat32(MaxTotalDelay);
        writer.WriteFloat32(MinHealth);
        writer.WriteFloat32(MaxHealth);
    }

    public void Deserialize(PacketReader reader)
    {
        MinDelay      = reader.ReadFloat32();
        MaxDelay      = reader.ReadFloat32();
        MaxTotalDelay = reader.ReadFloat32();
        MinHealth     = reader.ReadFloat32();
        MaxHealth     = reader.ReadFloat32();
    }
}
