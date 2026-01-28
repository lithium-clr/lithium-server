using Lithium.Server.Core.Networking.Protocol;
using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking;

public sealed record SoundEvent : PacketObject
{
    [PacketProperty(FixedIndex = 0)]
    public float Volume { get; init; }

    [PacketProperty(FixedIndex = 1)]
    public float Pitch { get; init; }

    [PacketProperty(FixedIndex = 2)]
    public float MusicDuckingVolume { get; init; }

    [PacketProperty(FixedIndex = 3)]
    public float AmbientDuckingVolume { get; init; }

    [PacketProperty(FixedIndex = 4)]
    public int MaxInstance { get; init; }

    [PacketProperty(FixedIndex = 5)]
    public bool PreventSoundInterruption { get; init; }

    [PacketProperty(FixedIndex = 6)]
    public float StartAttenuationDistance { get; init; }

    [PacketProperty(FixedIndex = 7)]
    public float MaxDistance { get; init; }

    [PacketProperty(FixedIndex = 8)]
    public int AudioCategory { get; init; }

    [PacketProperty(BitIndex = 0, OffsetIndex = 0)]
    public string? Id { get; init; }

    [PacketProperty(BitIndex = 1, OffsetIndex = 1)]
    public SoundEventLayer[]? Layers { get; init; }
}