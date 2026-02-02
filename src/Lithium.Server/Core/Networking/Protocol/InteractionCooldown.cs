using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 8,
    VariableFieldCount = 2,
    VariableBlockStart = 16,
    MaxSize = 32768026
)]
public sealed class InteractionCooldown : INetworkSerializable
{
    [JsonPropertyName("cooldownId")]
    public string? CooldownId { get; set; }

    [JsonPropertyName("cooldown")]
    public float Cooldown { get; set; }

    [JsonPropertyName("clickBypass")]
    public bool ClickBypass { get; set; }

    [JsonPropertyName("chargeTimes")]
    public float[]? ChargeTimes { get; set; }

    [JsonPropertyName("skipCooldownReset")]
    public bool SkipCooldownReset { get; set; }

    [JsonPropertyName("interruptRecharge")]
    public bool InterruptRecharge { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (CooldownId is not null) bits.SetBit(1);
        if (ChargeTimes is not null) bits.SetBit(2);

        writer.WriteBits(bits);

        writer.WriteFloat32(Cooldown);
        writer.WriteBoolean(ClickBypass);
        writer.WriteBoolean(SkipCooldownReset);
        writer.WriteBoolean(InterruptRecharge);

        var cooldownIdOffsetSlot = writer.ReserveOffset();
        var chargeTimesOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (CooldownId is not null)
        {
            writer.WriteOffsetAt(cooldownIdOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(CooldownId, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(cooldownIdOffsetSlot, -1);
        }

        if (ChargeTimes is not null)
        {
            writer.WriteOffsetAt(chargeTimesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(ChargeTimes.Length);
            foreach (var time in ChargeTimes)
            {
                writer.WriteFloat32(time);
            }
        }
        else
        {
            writer.WriteOffsetAt(chargeTimesOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        Cooldown = reader.ReadFloat32();
        ClickBypass = reader.ReadBoolean();
        SkipCooldownReset = reader.ReadBoolean();
        InterruptRecharge = reader.ReadBoolean();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            CooldownId = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            ChargeTimes = reader.ReadArrayAt(offsets[1], r => r.ReadFloat32());
        }
    }
}