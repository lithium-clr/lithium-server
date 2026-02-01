using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

// TODO: Implement a JsonConverter for polymorphic deserialization
// [JsonConverter(typeof(InteractionConverter))]
[Packet(MaxSize = 1677721605)]
public class Interaction : INetworkSerializable
{
    [JsonPropertyName("typeId")] public virtual int TypeId { get; protected set; }

    [JsonPropertyName("waitForDataFrom")] public WaitForDataFrom WaitForDataFrom { get; set; } = WaitForDataFrom.Client;
    [JsonPropertyName("effects")] public InteractionEffects? Effects { get; set; }

    [JsonPropertyName("horizontalSpeedMultiplier")]
    public float HorizontalSpeedMultiplier { get; set; }

    [JsonPropertyName("runTime")] public float RunTime { get; set; }

    [JsonPropertyName("cancelOnItemChange")]
    public bool CancelOnItemChange { get; set; }

    [JsonPropertyName("settings")] public Dictionary<GameMode, InteractionSettings>? Settings { get; set; }
    [JsonPropertyName("rules")] public InteractionRules? Rules { get; set; }
    [JsonPropertyName("tags")] public int[]? Tags { get; set; }
    [JsonPropertyName("camera")] public InteractionCameraSettings? Camera { get; set; }

    public virtual void Serialize(PacketWriter writer)
    {
        writer.WriteInt32(TypeId);
    }

    public virtual void Deserialize(PacketReader reader)
    {
        TypeId = reader.ReadInt32();
    }

    public static Interaction ReadPolymorphic(PacketReader reader)
    {
        var typeId = reader.ReadVarInt32();

        Interaction interaction = typeId switch
        {
            // This is where concrete interaction types will be registered.
            // Example:
            // 0 => new SimpleBlockInteraction(),
            // 1 => new SimpleInteraction(),
            _ => throw new NotSupportedException($"Interaction with type ID {typeId} is not supported.")
        };

        interaction.Deserialize(reader);
        return interaction;
    }

    public void SerializeWithTypeId(PacketWriter writer)
    {
        writer.WriteVarInt(TypeId);
        Serialize(writer);
    }
}