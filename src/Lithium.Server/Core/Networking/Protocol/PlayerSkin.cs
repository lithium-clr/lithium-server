namespace Lithium.Server.Core.Networking.Protocol;

public sealed record PlayerSkin : INetworkSerializable
{
    public string? BodyCharacteristic { get; init; }
    public string? Underwear { get; init; }
    public string? Face { get; init; }
    public string? Eyes { get; init; }
    public string? Ears { get; init; }
    public string? Mouth { get; init; }
    public string? FacialHair { get; init; }
    public string? Haircut { get; init; }
    public string? Eyebrows { get; init; }
    public string? Pants { get; init; }
    public string? Overpants { get; init; }
    public string? Undertop { get; init; }
    public string? Overtop { get; init; }
    public string? Shoes { get; init; }
    public string? HeadAccessory { get; init; }
    public string? FaceAccessory { get; init; }
    public string? EarAccessory { get; init; }
    public string? SkinFeature { get; init; }
    public string? Gloves { get; init; }
    public string? Cape { get; init; }

    public void Serialize(PacketWriter writer)
    {
        throw new NotImplementedException();
    }

    public void Deserialize(PacketReader reader)
    {
        throw new NotImplementedException();
    }
}