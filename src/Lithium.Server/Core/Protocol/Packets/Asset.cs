using System.Text;

namespace Lithium.Server.Core.Protocol.Packets;

public readonly struct Asset(string hash, string name)
{
    private const int FixedBlockSize = 64;
    private const int MaxNameLength = 512;

    public readonly string Hash = hash;
    public readonly string Name = name;

    public static Asset Deserialize(ReadOnlySpan<byte> buffer, out int bytesRead)
    {
        var reader = new PacketReader(buffer);
        
        var hash = reader.ReadFixedString(FixedBlockSize);
        var name = reader.ReadVarString();

        if (name.Length > MaxNameLength)
            throw new InvalidDataException($"Name exceeds max length {MaxNameLength}. Got {name.Length}");

        bytesRead = reader.Offset;
        return new Asset(hash, name);
    }

    public void Serialize(Stream stream)
    {
        Span<byte> hashBytes = stackalloc byte[FixedBlockSize];
        
        if (!string.IsNullOrEmpty(Hash))
            Encoding.ASCII.GetBytes(Hash, hashBytes);
        
        stream.Write(hashBytes);
        PacketSerializer.WriteVarString(stream, Name);
    }
}
