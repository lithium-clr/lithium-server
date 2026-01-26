using Lithium.Server.Core.Protocol;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

public sealed class WorldLoadFinishedPacket : IPacket<WorldLoadFinishedPacket>
{
    public static int Id => 22;

    public void Serialize(Stream stream)
    {
    }
    
    public static WorldLoadFinishedPacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        return new WorldLoadFinishedPacket();
    }
}
