namespace Lithium.Server.Core.Protocol;

public readonly record struct BlobData(ReadOnlyMemory<byte> Data)
{
    public static BlobData Empty { get; } = new(ReadOnlyMemory<byte>.Empty);

    public bool HasData => !Data.IsEmpty;
    public int Length => Data.Length;
}