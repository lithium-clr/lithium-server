using System.Security.Cryptography;
using Lithium.Server.Core.Networking;
using Lithium.Server.Core.Networking.Protocol;

namespace Lithium.Server.Core;

public abstract record CommonAsset(string Name, string Hash)
{
    private readonly Lock _blobLock = new();
    private Task<BlobData>? _blobTask;

    public string Name { get; } = Name.Replace('\\', '/');
    public string Hash { get; } = Hash.ToLowerInvariant();

    protected CommonAsset(string name, byte[] data) 
        : this(name, ComputeHash(data))
    {
        _blobTask = Task.FromResult(new BlobData(data));
    }

    public Task<BlobData> GetBlobAsync()
    {
        lock (_blobLock)
        {
            if (_blobTask is not null)
                return _blobTask;

            _blobTask = ReadBlobAsync();
            return _blobTask;
        }
    }

    protected abstract Task<BlobData> ReadBlobAsync();

    public Asset ToPacket()
    {
        return new Asset
        {
            Name = Name,
            Hash = Hash
        };
    }

    private static string ComputeHash(ReadOnlySpan<byte> bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}