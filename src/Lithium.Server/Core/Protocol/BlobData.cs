namespace Lithium.Server.Core.Protocol;

public readonly record struct BlobData(byte[]? Data)
{
    public bool HasData => Data is not null;
    public int Length => Data?.Length ?? 0;
}