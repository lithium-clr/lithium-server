using Lithium.Server.Core.Networking.Protocol;
using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking;

public sealed record SoundEventLayer : PacketObject
{
    [PacketProperty(FixedIndex = 0)]
    public float Volume { get; init; } = 1f;

    [PacketProperty(FixedIndex = 1)]
    public float StartDelay { get; init; }

    [PacketProperty(FixedIndex = 2)]
    public bool Looping { get; init; }

    [PacketProperty(FixedIndex = 3)]
    public int Probability { get; init; } = 100;

    [PacketProperty(FixedIndex = 4)]
    public float ProbabilityRerollDelay { get; init; } = 1f;

    [PacketProperty(FixedIndex = 5)]
    public SoundEventLayerRandomSettings RandomSettings { get; init; } = new();

    [PacketProperty(FixedIndex = 6)]
    public int RoundRobinHistorySize { get; init; }

    [PacketProperty(OffsetIndex = 0)]
    public string[]? Files { get; init; }

    public sealed record SoundEventLayerRandomSettings : PacketObject
    {
        [PacketProperty(FixedIndex = 0)]
        public float MinVolume { get; init; } = 1f;

        [PacketProperty(FixedIndex = 1)]
        public float MaxVolume { get; init; } = 1f;

        [PacketProperty(FixedIndex = 2)]
        public float MinPitch { get; init; } = 1f;

        [PacketProperty(FixedIndex = 3)]
        public float MaxPitch { get; init; } = 1f;

        [PacketProperty(FixedIndex = 4)]
        public float MaxStartOffset { get; init; }
    }
}