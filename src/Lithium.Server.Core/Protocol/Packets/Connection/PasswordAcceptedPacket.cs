namespace Lithium.Server.Core.Protocol.Packets.Connection;

public struct PasswordAcceptedPacket : IPacket<PasswordAcceptedPacket>
{
    public static int Id => 16;

    public void Serialize(Stream stream)
    {
    }
}