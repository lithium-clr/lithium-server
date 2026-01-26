using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

public sealed record PlayerSkin : PacketObject
{
    // Java: NULLABLE_BIT_FIELD_SIZE = 3, VARIABLE_FIELD_COUNT = 20, VARIABLE_BLOCK_START = 83
    
    [PacketProperty(BitIndex = 0, OffsetIndex = 0)]
    public string? BodyCharacteristic { get; init; }

    [PacketProperty(BitIndex = 1, OffsetIndex = 1)]
    public string? Underwear { get; init; }

    [PacketProperty(BitIndex = 2, OffsetIndex = 2)]
    public string? Face { get; init; }

    [PacketProperty(BitIndex = 3, OffsetIndex = 3)]
    public string? Eyes { get; init; }

    [PacketProperty(BitIndex = 4, OffsetIndex = 4)]
    public string? Ears { get; init; }

    [PacketProperty(BitIndex = 5, OffsetIndex = 5)]
    public string? Mouth { get; init; }

    [PacketProperty(BitIndex = 6, OffsetIndex = 6)]
    public string? FacialHair { get; init; }

    [PacketProperty(BitIndex = 7, OffsetIndex = 7)]
    public string? Haircut { get; init; }

    [PacketProperty(BitIndex = 8, OffsetIndex = 8)]
    public string? Eyebrows { get; init; }

    [PacketProperty(BitIndex = 9, OffsetIndex = 9)]
    public string? Pants { get; init; }

    [PacketProperty(BitIndex = 10, OffsetIndex = 10)]
    public string? Overpants { get; init; }

    [PacketProperty(BitIndex = 11, OffsetIndex = 11)]
    public string? Undertop { get; init; }

    [PacketProperty(BitIndex = 12, OffsetIndex = 12)]
    public string? Overtop { get; init; }

    [PacketProperty(BitIndex = 13, OffsetIndex = 13)]
    public string? Shoes { get; init; }

    [PacketProperty(BitIndex = 14, OffsetIndex = 14)]
    public string? HeadAccessory { get; init; }

    [PacketProperty(BitIndex = 15, OffsetIndex = 15)]
    public string? FaceAccessory { get; init; }

    [PacketProperty(BitIndex = 16, OffsetIndex = 16)]
    public string? EarAccessory { get; init; }

    [PacketProperty(BitIndex = 17, OffsetIndex = 17)]
    public string? SkinFeature { get; init; }

    [PacketProperty(BitIndex = 18, OffsetIndex = 18)]
    public string? Gloves { get; init; }

    [PacketProperty(BitIndex = 19, OffsetIndex = 19)]
    public string? Cape { get; init; }
}