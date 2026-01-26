namespace Lithium.Server.Core.Protocol.Packets;

public sealed class RequestCommonAssetsRebuildPacket : IPacket<RequestCommonAssetsRebuildPacket>
{
    public static int Id => 28;

    public void Serialize(Stream stream)
    {
    }
    
    public static RequestCommonAssetsRebuildPacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        return new RequestCommonAssetsRebuildPacket();
    }

}
