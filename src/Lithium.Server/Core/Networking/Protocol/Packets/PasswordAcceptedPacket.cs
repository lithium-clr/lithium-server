using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 16, MaxSize = 0)]
public sealed class PasswordAcceptedPacket : Packet;
