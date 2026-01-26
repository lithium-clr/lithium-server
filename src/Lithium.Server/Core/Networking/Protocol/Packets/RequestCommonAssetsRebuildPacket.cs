using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(28, MaxSize = 0)]
public sealed class RequestCommonAssetsRebuildPacket : Packet;
