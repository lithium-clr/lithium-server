using System.Text.Json.Serialization;
using Lithium.Server.Core.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 6,
    VariableFieldCount = 6,
    VariableBlockStart = 30,
    MaxSize = 1677721600
)]
public sealed class RootInteraction : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("interactions")]
    public int[]? Interactions { get; set; }

    [JsonPropertyName("cooldown")]
    public InteractionCooldown? Cooldown { get; set; }

    [JsonPropertyName("settings")]
    [JsonConverter(typeof(EnumKeyDictionaryConverter<GameMode, RootInteractionSettings>))]
    public Dictionary<GameMode, RootInteractionSettings>? Settings { get; set; }

    [JsonPropertyName("rules")]
    public InteractionRules? Rules { get; set; }

    [JsonPropertyName("tags")]
    public int[]? Tags { get; set; }

    [JsonPropertyName("clickQueuingTimeout")]
    public float ClickQueuingTimeout { get; set; }

    [JsonPropertyName("requireNewClick")]
    public bool RequireNewClick { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Id is not null) bits.SetBit(1);
        if (Interactions is not null) bits.SetBit(2);
        if (Cooldown is not null) bits.SetBit(4);
        if (Settings is not null) bits.SetBit(8);
        if (Rules is not null) bits.SetBit(16);
        if (Tags is not null) bits.SetBit(32);

        writer.WriteBits(bits);

        writer.WriteFloat32(ClickQueuingTimeout);
        writer.WriteBoolean(RequireNewClick);

        var idOffsetSlot = writer.ReserveOffset();
        var interactionsOffsetSlot = writer.ReserveOffset();
        var cooldownOffsetSlot = writer.ReserveOffset();
        var settingsOffsetSlot = writer.ReserveOffset();
        var rulesOffsetSlot = writer.ReserveOffset();
        var tagsOffsetSlot = writer.ReserveOffset();

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

        if (Interactions is not null)
        {
            writer.WriteOffsetAt(interactionsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Interactions.Length);
            foreach (var item in Interactions)
            {
                writer.WriteInt32(item);
            }
        }
        else
        {
            writer.WriteOffsetAt(interactionsOffsetSlot, -1);
        }

        if (Cooldown is not null)
        {
            writer.WriteOffsetAt(cooldownOffsetSlot, writer.Position - varBlockStart);
            Cooldown.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(cooldownOffsetSlot, -1);
        }

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
        else
        {
            writer.WriteOffsetAt(settingsOffsetSlot, -1);
        }

        if (Rules is not null)
        {
            writer.WriteOffsetAt(rulesOffsetSlot, writer.Position - varBlockStart);
            Rules.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(rulesOffsetSlot, -1);
        }

        if (Tags is not null)
        {
            writer.WriteOffsetAt(tagsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Tags.Length);
            foreach (var item in Tags)
            {
                writer.WriteInt32(item);
            }
        }
        else
        {
            writer.WriteOffsetAt(tagsOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        ClickQueuingTimeout = reader.ReadFloat32();
        RequireNewClick = reader.ReadBoolean();

        var offsets = reader.ReadOffsets(6);

        if (bits.IsSet(1))
        {
            Id = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            Interactions = reader.ReadArrayAt(offsets[1], r => r.ReadInt32());
        }

        if (bits.IsSet(4))
        {
            Cooldown = reader.ReadObjectAt<InteractionCooldown>(offsets[2]);
        }

        if (bits.IsSet(8))
        {
            Settings = reader.ReadDictionaryAt(
                offsets[3],
                r => r.ReadEnum<GameMode>(),
                r =>
                {
                    var s = new RootInteractionSettings();
                    s.Deserialize(r);
                    return s;
                }
            );
        }

        if (bits.IsSet(16))
        {
            Rules = reader.ReadObjectAt<InteractionRules>(offsets[4]);
        }

        if (bits.IsSet(32))
        {
            Tags = reader.ReadArrayAt(offsets[5], r => r.ReadInt32());
        }
    }
}