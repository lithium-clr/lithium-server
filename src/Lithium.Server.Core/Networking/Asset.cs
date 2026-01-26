using System.Text;
using Lithium.Server.Core.Networking.Protocol;

namespace Lithium.Server.Core.Networking;

public sealed class Asset
{
    private const int FixedBlockSize = 64;
    private const int MaxNameLength = 512;

    public string Hash { get; init; } = null!;
    public string Name { get; init; } = null!;

    public void Serialize(Stream stream)
    {
        Span<byte> hashBytes = stackalloc byte[FixedBlockSize];

        if (!string.IsNullOrEmpty(Hash))
            Encoding.ASCII.GetBytes(Hash, hashBytes);

        stream.Write(hashBytes);
        PacketSerializer.WriteVarString(stream, Name);
    }
    
    public static Asset Deserialize(ReadOnlySpan<byte> buffer, out int bytesRead)
    {
        var reader = new PacketReader(buffer);

        var hash = reader.ReadFixedString(FixedBlockSize);
        var name = reader.ReadVarString();

        if (name.Length > MaxNameLength)
            throw new InvalidDataException($"Name exceeds max length {MaxNameLength}. Got {name.Length}");

        bytesRead = reader.Offset;

        return new Asset
        {
            Hash = hash,
            Name = name
        };
    }
}