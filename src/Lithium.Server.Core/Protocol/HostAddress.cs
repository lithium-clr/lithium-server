namespace Lithium.Server.Core.Protocol;

public sealed class HostAddress
{
    public string Host { get; private set; }
    public short Port { get; private set; }

    private HostAddress(string host, short port)
    {
        Host = host;
        Port = port;
    }

    private HostAddress()
    {
        
    }

    public static HostAddress Deserialize(byte[] buffer, int offset, out int bytesRead)
    {
        var obj = new HostAddress
        {
            Port = BitConverter.ToInt16(buffer, offset)
        };

        var stringPos = offset + 2;
        obj.Host = PacketSerializer.ReadVarString(buffer, stringPos, out var hostVarIntLen);

        bytesRead = 2 + hostVarIntLen;
        return obj;
    }

    public void Serialize(Stream stream)
    {
        stream.Write(BitConverter.GetBytes(Port));
        PacketSerializer.WriteVarString(stream, Host);
    }
}