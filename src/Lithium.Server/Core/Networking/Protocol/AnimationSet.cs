using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 9,
    VariableFieldCount = 2,
    VariableBlockStart = 17,
    MaxSize = 1677721600
)]
public sealed class AnimationSet : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("animations")]
    public Animation[]? Animations { get; set; }

    [JsonPropertyName("nextAnimationDelay")]
    public RangeFloat? NextAnimationDelay { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (NextAnimationDelay is not null) bits.SetBit(1);
        if (Id is not null) bits.SetBit(2);
        if (Animations is not null) bits.SetBit(4);

        writer.WriteBits(bits);

        // Fixed Block
        if (NextAnimationDelay is not null)
        {
            NextAnimationDelay.Value.Serialize(writer);
        }
        else
        {
            writer.WriteInt64(0); // 8 bytes padding
        }

        // Reserve Offsets
        var idOffset = writer.ReserveOffset();
        var animationsOffset = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // Variable Block
        if (Id is not null)
        {
            writer.WriteOffsetAt(idOffset, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Id, 4096000);
        }
        else
        {
            writer.WriteOffsetAt(idOffset, -1);
        }

        if (Animations is not null)
        {
            writer.WriteOffsetAt(animationsOffset, writer.Position - varBlockStart);
            writer.WriteVarInt(Animations.Length);
            foreach (var animation in Animations)
            {
                animation.Serialize(writer);
            }
        }
        else
        {
            writer.WriteOffsetAt(animationsOffset, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        // Fixed Block
        if (bits.IsSet(1))
        {
            NextAnimationDelay = reader.ReadObject<RangeFloat>();
        }
        else
        {
            reader.ReadInt64(); // Skip 8 bytes padding
        }

        // Read Offsets
        var offsets = reader.ReadOffsets(2);

        // Variable Block
        if (bits.IsSet(2))
        {
            Id = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(4))
        {
            Animations = reader.ReadObjectArrayAt<Animation>(offsets[1]);
        }
    }
}
