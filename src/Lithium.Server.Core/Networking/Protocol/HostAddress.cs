namespace Lithium.Server.Core.Networking.Protocol;

public sealed record HostAddress : INetworkSerializable
{
    public short Port { get; set; }
    public string Host { get; set; } = string.Empty;

    public HostAddress()
    {
    }

    public HostAddress(string host, short port)
    {
        Host = host;
        Port = port;
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt16(Port);
        writer.WriteUtf8String(Host, 4096000);
    }

    public void Deserialize(PacketReader reader)
    {
        Port = reader.ReadInt16();
        Host = reader.ReadUtf8String();
    }
}