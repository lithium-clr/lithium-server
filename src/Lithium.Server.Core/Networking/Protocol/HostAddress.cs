using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed record HostAddress : PacketObject
{
    [PacketProperty(FixedIndex = 0)] public short Port { get; set; }
    [PacketProperty(OffsetIndex = 0)] public string Host { get; set; } = string.Empty;

    public HostAddress() { }

    public HostAddress(string host, short port)
    {
        Host = host;
        Port = port;
    }
}
