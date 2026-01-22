namespace Lithium.Server.Core.Protocol;

public interface INetworkSerializable<out T>
{
    T ToPacket();
}