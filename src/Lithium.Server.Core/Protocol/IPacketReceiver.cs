// namespace Lithium.Server.Core.Protocol;
//
// public interface IPacketReceiver
// {
//     void Write(IPacket packet);
//     void WriteNoCache(IPacket packet);
// }
//
// public abstract class PacketHandler : IPacketReceiver
// {
//     public const int MaxPacketId = 512;
//     
//     // protected readonly DisconnectReason DisconnectReason = new();
//     
//     public void Write(IPacket packet)
//     {
//         throw new NotImplementedException();
//     }
//
//     public void WriteNoCache(IPacket packet)
//     {
//         throw new NotImplementedException();
//     }
// }