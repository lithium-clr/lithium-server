using System.Net.Quic;

namespace Hytale.Server.Core.Networking;

public readonly struct PacketContext(QuicConnection connection)
{
    public readonly QuicConnection Connection = connection;
}