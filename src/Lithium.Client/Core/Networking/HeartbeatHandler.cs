// using Lithium.Core.Networking;
// using Lithium.Core.Networking.Packets;
// using Microsoft.Extensions.Logging;
//
// namespace Lithium.Client.Core.Networking;
//
// public sealed class HeartbeatHandler(
//     ILogger<HeartbeatHandler> logger
// ) : IPacketHandler<HeartbeatPacket>
// {
//     public void Handle(in HeartbeatPacket p, PacketContext ctx)
//     {
//         logger.LogInformation("Heartbeat received");
//     }
// }