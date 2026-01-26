namespace Lithium.Server.Core.Protocol.Packets;

public sealed class PasswordAcceptedPacket : IPacket<PasswordAcceptedPacket>
{
    public static int Id => 16;

    public void Serialize(Stream stream)
    {
        // Nothing to serialize, but we need to override this method to avoid to trigger a NotImplementedException
    }
}
