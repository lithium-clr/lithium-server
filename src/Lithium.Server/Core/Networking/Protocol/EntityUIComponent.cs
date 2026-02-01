using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class EntityUIComponent : INetworkSerializable
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<EntityUIType>))]
    public EntityUIType Type { get; set; } = EntityUIType.EntityStat;

    [JsonPropertyName("hitboxOffset")]
    public Vector2Float? HitboxOffset { get; set; }

    [JsonPropertyName("unknown")]
    public bool Unknown { get; set; }

    [JsonPropertyName("entityStatIndex")]
    public int EntityStatIndex { get; set; }

    [JsonPropertyName("combatTextRandomPositionOffsetRange")]
    public RangeVector2Float? CombatTextRandomPositionOffsetRange { get; set; }

    [JsonPropertyName("combatTextViewportMargin")]
    public float CombatTextViewportMargin { get; set; }

    [JsonPropertyName("combatTextDuration")]
    public float CombatTextDuration { get; set; }

    [JsonPropertyName("combatTextHitAngleModifierStrength")]
    public float CombatTextHitAngleModifierStrength { get; set; }

    [JsonPropertyName("combatTextFontSize")]
    public float CombatTextFontSize { get; set; }

    [JsonPropertyName("combatTextColor")]
    public Color? CombatTextColor { get; set; }

    [JsonPropertyName("combatTextAnimationEvents")]
    public CombatTextEntityUIComponentAnimationEvent[]? CombatTextAnimationEvents { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (HitboxOffset is not null) bits.SetBit(1);
        if (CombatTextRandomPositionOffsetRange is not null) bits.SetBit(2);
        if (CombatTextColor is not null) bits.SetBit(4);
        if (CombatTextAnimationEvents is not null) bits.SetBit(8);
        writer.WriteBits(bits);

        writer.WriteEnum(Type);

        if (HitboxOffset is not null) HitboxOffset.Serialize(writer); else writer.WriteZero(8);
        writer.WriteBoolean(Unknown);
        writer.WriteInt32(EntityStatIndex);
        if (CombatTextRandomPositionOffsetRange is not null) CombatTextRandomPositionOffsetRange.Serialize(writer); else writer.WriteZero(17);
        writer.WriteFloat32(CombatTextViewportMargin);
        writer.WriteFloat32(CombatTextDuration);
        writer.WriteFloat32(CombatTextHitAngleModifierStrength);
        writer.WriteFloat32(CombatTextFontSize);
        if (CombatTextColor is not null) CombatTextColor.Serialize(writer); else writer.WriteZero(3);

        if (CombatTextAnimationEvents is not null)
        {
            writer.WriteVarInt(CombatTextAnimationEvents.Length);
            foreach (var animationEvent in CombatTextAnimationEvents)
            {
                animationEvent.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        Type = reader.ReadEnum<EntityUIType>();

        if (bits.IsSet(1)) HitboxOffset = reader.ReadObject<Vector2Float>(); else { reader.ReadFloat32(); reader.ReadFloat32(); }
        Unknown = reader.ReadBoolean();
        EntityStatIndex = reader.ReadInt32();
        if (bits.IsSet(2)) CombatTextRandomPositionOffsetRange = reader.ReadObject<RangeVector2Float>(); else for(int i=0; i<17; i++) reader.ReadUInt8();
        CombatTextViewportMargin = reader.ReadFloat32();
        CombatTextDuration = reader.ReadFloat32();
        CombatTextHitAngleModifierStrength = reader.ReadFloat32();
        CombatTextFontSize = reader.ReadFloat32();
        if (bits.IsSet(4)) CombatTextColor = reader.ReadObject<Color>(); else { reader.ReadUInt8(); reader.ReadUInt8(); reader.ReadUInt8(); }

        if (bits.IsSet(8))
        {
            var count = reader.ReadVarInt32();
            CombatTextAnimationEvents = new CombatTextEntityUIComponentAnimationEvent[count];
            for (var i = 0; i < count; i++)
            {
                CombatTextAnimationEvents[i] = reader.ReadObject<CombatTextEntityUIComponentAnimationEvent>();
            }
        }
    }
}
