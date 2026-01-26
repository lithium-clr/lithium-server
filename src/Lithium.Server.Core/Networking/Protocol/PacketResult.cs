using System.Diagnostics.CodeAnalysis;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed record PacketResult(int PacketId, string? PacketName, Packet? Packet, Exception? Exception)
{
    [MemberNotNullWhen(true, nameof(Packet))]
    [MemberNotNullWhen(true, nameof(PacketName))]
    [MemberNotNullWhen(false, nameof(Exception))]
    public bool Success =>
        Packet is not null;

    [MemberNotNullWhen(false, nameof(Packet))]
    [MemberNotNullWhen(false, nameof(PacketName))]
    [MemberNotNullWhen(true, nameof(Exception))]
    public bool Failure =>
        Packet is null;

    public static PacketResult Ok(int packetId, string packetName, Packet packet)
    {
        return new PacketResult(packetId, packetName, packet, null);
    }

    public static PacketResult No(Exception exception)
    {
        return new PacketResult(-1, null, null, exception);
    }
}