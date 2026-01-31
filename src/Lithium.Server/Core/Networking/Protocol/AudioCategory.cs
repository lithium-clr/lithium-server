namespace Lithium.Server.Core.Networking.Protocol;

public sealed record AudioCategory : INetworkSerializable
{
    public string? Id { get; init; }
    public float Volume { get; init; }

    public void Serialize(PacketWriter writer)
    {
        throw new NotImplementedException();
    }

    public void Deserialize(PacketReader reader)
    {
        throw new NotImplementedException();
    }
}