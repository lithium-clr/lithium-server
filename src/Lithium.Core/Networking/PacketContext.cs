using System.Net.Quic;

namespace Lithium.Core.Networking;

public readonly struct PacketContext(QuicConnection connection, QuicStream stream)
{
    public readonly QuicConnection Connection = connection;
    public readonly QuicStream Stream = stream;
}