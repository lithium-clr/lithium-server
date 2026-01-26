using Lithium.Server.Core.Protocol;
using Lithium.Server.Core.Protocol.Packets;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

public sealed class RequestAssetsPacket : IPacket<RequestAssetsPacket>
{
    public static int Id => 23;
    public static bool IsCompressed => true;

    public Asset[]? Assets { get; init; }

    public static RequestAssetsPacket Deserialize(ReadOnlySpan<byte> buffer)
    {
        Console.WriteLine("(RequestAssetsPacket) -> Deserialize");
        
        var reader = new PacketReader(buffer);
        var nullBits = reader.ReadByte();

        Asset[]? assets = null;

        Console.WriteLine("(RequestAssetsPacket) -> nullBits: " + nullBits);
        
        if ((nullBits & 1) is not 0)
        {
            var count = reader.ReadVarInt();
            Console.WriteLine("(RequestAssetsPacket) -> count: " + count);
            
            if (count < 0)
                throw new InvalidDataException($"Assets count cannot be negative. Got {count}");
            
            if (count > 4096000)
                throw new InvalidDataException($"Assets exceeds max length 4096000. Got {count}");

            assets = new Asset[count];
            
            // for (var i = 0; i < count; i++)
            //     assets[i] = Asset.Deserialize(ref reader);
            
            var currentOffset = reader.Offset;

            for (var i = 0; i < count; i++)
            {
                assets[i] = Asset.Deserialize(buffer[currentOffset..], out var bytesRead);
                currentOffset += bytesRead;
            }
            
            Console.WriteLine("(RequestAssetsPacket) -> Assets count: " + assets.Length);
        }

        return new RequestAssetsPacket
        {
            Assets = assets
        };
    }

    public void Serialize(Stream stream)
    {
        byte nullBits = 0;
        
        if (Assets is not null)
            nullBits |= 1;

        stream.WriteByte(nullBits);
        
        if (Assets is not null)
        {
            if (Assets.Length > 4096000)
                throw new InvalidDataException($"Assets exceeds max length 4096000. Got {Assets.Length}");

            PacketSerializer.WriteVarInt(stream, Assets.Length);
            
            foreach (var asset in Assets)
                asset.Serialize(stream);
        }
    }
}
