using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Lithium.Server.Core.Protocol.Packets;
using Lithium.SourceGenerators.Attributes;

namespace Lithium.Server.Core.Protocol;

public abstract partial class CommonAsset : INetworkSerializable<Asset>
{
    public const int HashLength = 64;
    public static readonly Regex HashPattern = MyRegex();

    protected readonly WeakReference<Task<byte[]?>> Blob;
    protected WeakReference<Asset>? CachedPacket;

    [ToStringInclude] public string Name { get; }
    [ToStringInclude] public string Hash { get; }

    protected CommonAsset(string name, byte[]? bytes)
        : this(name, bytes is null ? string.Empty : ComputeHash(bytes), bytes)
    {
    }

    protected CommonAsset(string name, string hash, byte[]? bytes)
    {
        Name = name.Replace('\\', '/');
        Hash = hash.ToLowerInvariant();
        Blob = new WeakReference<Task<byte[]?>>(Task.FromResult(bytes));
    }

    public Task<byte[]?> GetBlob()
    {
        if (Blob.TryGetTarget(out var task))
            return task;

        task = GetBlob0();
        Blob.SetTarget(task);

        return task;
    }

    protected abstract Task<byte[]> GetBlob0();

    public Asset ToPacket()
    {
        if (CachedPacket?.TryGetTarget(out var packet) is true)
            return packet;

        packet = new Asset { Name = Name, Hash = Hash };
        CachedPacket = new WeakReference<Asset>(packet);

        return packet;
    }

    public override bool Equals(object? obj)
        => obj is CommonAsset a && a.Name == Name && a.Hash == Hash;

    public override int GetHashCode()
        => HashCode.Combine(Name, Hash);

    public static string ComputeHash(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    [GeneratedRegex("^[A-Fa-f0-9]{64}$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}