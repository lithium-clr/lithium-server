using System.Buffers.Binary;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class HostAddress(string host, short port)
{
    public string Host { get; } = host;
    public short Port { get; } = port;

    public static HostAddress Deserialize(ReadOnlySpan<byte> buffer, out int bytesRead)
    {
        var reader = new PacketReader(buffer);
        var port = reader.ReadInt16();
        var host = reader.ReadVarString();

        bytesRead = reader.Offset;
        return new HostAddress(host, port);
    }

    public void Serialize(Stream stream)
    {
        Span<byte> portBuffer = stackalloc byte[2];
        BinaryPrimitives.WriteInt16LittleEndian(portBuffer, Port);
        stream.Write(portBuffer);
        
        PacketSerializer.WriteVarString(stream, Host);
    }
}
