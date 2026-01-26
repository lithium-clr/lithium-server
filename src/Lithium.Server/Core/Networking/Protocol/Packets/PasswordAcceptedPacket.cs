using Lithium.Server.Core.Protocol;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

public sealed class PasswordAcceptedPacket : IPacket<PasswordAcceptedPacket>
{
    public static int Id => 16;

    public void Serialize(Stream stream)
    {
        // Nothing to serialize, but we need to override this method to avoid to trigger a NotImplementedException
    }
}
