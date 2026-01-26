using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

public sealed record PlayerSkin : PacketObject
{
    [PacketProperty(BitIndex = 0, OffsetIndex = 0)]
    public string? BodyCharacteristic { get; set; }

    [PacketProperty(BitIndex = 1, OffsetIndex = 1)]
    public string? Underwear { get; set; }

    [PacketProperty(BitIndex = 2, OffsetIndex = 2)]
    public string? Face { get; set; }

    [PacketProperty(BitIndex = 3, OffsetIndex = 3)]
    public string? Eyes { get; set; }

    [PacketProperty(BitIndex = 4, OffsetIndex = 4)]
    public string? Ears { get; set; }

    [PacketProperty(BitIndex = 5, OffsetIndex = 5)]
    public string? Mouth { get; set; }

    [PacketProperty(BitIndex = 6, OffsetIndex = 6)]
    public string? FacialHair { get; set; }

    [PacketProperty(BitIndex = 7, OffsetIndex = 7)]
    public string? Haircut { get; set; }

    [PacketProperty(BitIndex = 8, OffsetIndex = 8)]
    public string? Eyebrows { get; set; }

    [PacketProperty(BitIndex = 9, OffsetIndex = 9)]
    public string? Pants { get; set; }

    [PacketProperty(BitIndex = 10, OffsetIndex = 10)]
    public string? OverPants { get; set; }

    [PacketProperty(BitIndex = 11, OffsetIndex = 11)]
    public string? UnderTop { get; set; }

    [PacketProperty(BitIndex = 12, OffsetIndex = 12)]
    public string? Overtop { get; set; }

    [PacketProperty(BitIndex = 13, OffsetIndex = 13)]
    public string? Shoes { get; set; }

    [PacketProperty(BitIndex = 14, OffsetIndex = 14)]
    public string? HeadAccessory { get; set; }

    [PacketProperty(BitIndex = 15, OffsetIndex = 15)]
    public string? FaceAccessory { get; set; }

    [PacketProperty(BitIndex = 16, OffsetIndex = 16)]
    public string? EarAccessory { get; set; }

    [PacketProperty(BitIndex = 17, OffsetIndex = 17)]
    public string? SkinFeature { get; set; }

    [PacketProperty(BitIndex = 18, OffsetIndex = 18)]
    public string? Gloves { get; set; }

    [PacketProperty(BitIndex = 19, OffsetIndex = 19)]
    public string? Cape { get; set; }
}
