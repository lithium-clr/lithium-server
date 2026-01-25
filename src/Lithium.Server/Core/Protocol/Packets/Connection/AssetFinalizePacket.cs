namespace Lithium.Server.Core.Protocol.Packets.Connection;

public sealed class AssetFinalizePacket : IPacket<AssetFinalizePacket>
{
    public static int Id => 26;

    public void Serialize(Stream stream)
    {
    }
    
    public static AssetFinalizePacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        return new AssetFinalizePacket();
    }
}
