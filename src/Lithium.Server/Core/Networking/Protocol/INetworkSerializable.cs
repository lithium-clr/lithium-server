namespace Lithium.Server.Core.Networking.Protocol;

public interface INetworkSerializable<out T>
{
    T ToPacket();
}