using Lithium.Server.Core.Networking.Protocol;
using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking;

public sealed record BlockSoundSet : PacketObject
{
    [PacketProperty(BitIndex = 0, FixedIndex = 0)]
    public FloatRange? MoveInRepeatRange { get; init; } = FloatRange.Default;

    [PacketProperty(BitIndex = 1, OffsetIndex = 0)]
    public string? Id { get; init; }

    [PacketProperty(BitIndex = 2, OffsetIndex = 1)]
    public Dictionary<BlockSoundEvent, int>? SoundEventIndices { get; init; }
}