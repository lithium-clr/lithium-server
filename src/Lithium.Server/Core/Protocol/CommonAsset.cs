using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Lithium.Server.Core.Protocol.Packets;
using Lithium.SourceGenerators.Attributes;

namespace Lithium.Server.Core.Protocol;

public abstract partial class CommonAsset(string name, string hash, byte[]? bytes) : INetworkSerializable<Asset>
{
    public const int HashLength = 64;
    public static readonly Regex HashPattern = MyRegex();

    private readonly Lock _blobLock = new();
    private Task<BlobData>? _blobTask = Task.FromResult(new BlobData(bytes));
    private WeakReference<Asset>? _cachedPacket;

    [ToStringInclude] public string Name { get; } = name.Replace('\\', '/');
    [ToStringInclude] public string Hash { get; } = hash.ToLowerInvariant();

    protected CommonAsset(string name, byte[]? bytes)
        : this(name, bytes is null ? string.Empty : ComputeHash(bytes), bytes)
    {
    }

    public Task<BlobData> GetBlobAsync()
    {
        lock (_blobLock)
        {
            switch (_blobTask)
            {
                case { IsCompleted: false }:
                case { IsCompletedSuccessfully: true, Result.HasData: true }:
                    return _blobTask;
                default:
                    _blobTask = ReadBlobAsync();
                    return _blobTask;
            }
        }
    }

    protected abstract Task<BlobData> ReadBlobAsync();

    public Asset ToPacket()
    {
        if (_cachedPacket?.TryGetTarget(out var packet) is true)
            return packet;

        packet = new Asset { Name = Name, Hash = Hash };
        _cachedPacket = new WeakReference<Asset>(packet);

        return packet;
    }

    public override bool Equals(object? obj)
        => obj is CommonAsset a && a.Name == Name && a.Hash == Hash;

    public override int GetHashCode()
        => HashCode.Combine(Name, Hash);

    private static string ComputeHash(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    [GeneratedRegex("^[A-Fa-f0-9]{64}$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}