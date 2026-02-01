using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class Modifier : INetworkSerializable
{
    [JsonPropertyName("target")]
    [JsonConverter(typeof(JsonStringEnumConverter<ModifierTarget>))]
    public ModifierTarget Target { get; set; } = ModifierTarget.Min;

    [JsonPropertyName("calculationType")]
    [JsonConverter(typeof(JsonStringEnumConverter<CalculationType>))]
    public CalculationType CalculationType { get; set; } = CalculationType.Additive;

    [JsonPropertyName("amount")]
    public float Amount { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteEnum(Target);
        writer.WriteEnum(CalculationType);
        writer.WriteFloat32(Amount);
    }

    public void Deserialize(PacketReader reader)
    {
        Target = reader.ReadEnum<ModifierTarget>();
        CalculationType = reader.ReadEnum<CalculationType>();
        Amount = reader.ReadFloat32();
    }
}
