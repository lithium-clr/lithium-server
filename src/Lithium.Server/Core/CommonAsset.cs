using System.Security.Cryptography;
using Lithium.Server.Core.Networking;
using Lithium.Server.Core.Networking.Protocol;
using Lithium.Server.Core.Networking.Protocol.Packets;
using Lithium.SourceGenerators.Attributes;

namespace Lithium.Server.Core;

public abstract partial class CommonAsset(string name, string hash)
{
    private readonly Lock _blobLock = new();
    private Task<BlobData>? _blobTask;

    [ToStringInclude] public string Name { get; } = name.Replace('\\', '/');
    [ToStringInclude] public string Hash { get; } = hash.ToLowerInvariant();

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
    
    public override bool Equals(object? obj)
        => obj is CommonAsset a && a.Name == Name && a.Hash == Hash;

    public override int GetHashCode()
        => HashCode.Combine(Name, Hash);

    private static string ComputeHash(ReadOnlySpan<byte> bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}