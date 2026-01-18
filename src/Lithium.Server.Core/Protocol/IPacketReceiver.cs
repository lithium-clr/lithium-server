namespace Lithium.Server.Core.Protocol;

public interface IPacketReceiver
{
    void Write(IPacket packet);
    void WriteNoCache(IPacket packet);
}