namespace Lithium.Server.Core.Networking;

public interface INetworkSerializable
{
    void Serialize(PacketWriter writer)
    {
        throw new NotImplementedException();
    }

    void Deserialize(PacketReader reader)
    {
        throw new NotImplementedException();
    }
}