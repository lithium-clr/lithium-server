using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed record HostAddress(string Host, short Port) : PacketObject<HostAddress>
{
    [PacketProperty(FixedIndex = 1)] public string Host { get; } = Host;
    [PacketProperty(FixedIndex = 0)] public short Port { get; } = Port;

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteVarInt16(Port);
        writer.WriteVarString(Host);
    }

    public override HostAddress Deserialize(PacketReader reader, int offset)
    {
        var port = reader.ReadVarInt16At(offset);
        var host = reader.ReadVarStringAt(offset + sizeof(short));

        return new HostAddress(host, port);
    }
}