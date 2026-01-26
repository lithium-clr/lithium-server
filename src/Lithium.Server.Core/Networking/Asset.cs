using Lithium.Server.Core.Networking.Protocol;
using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking;

public sealed record Asset : PacketObject
{
    private const int FixedBlockSize = 64;

    [PacketProperty(FixedIndex = 0, FixedSize = FixedBlockSize)]
    public string Hash { get; set; } = string.Empty;

    [PacketProperty(OffsetIndex = 0)]
    public string Name { get; set; } = string.Empty;

    public Asset() { }

    public Asset(string hash, string name)
    {
        Hash = hash;
        Name = name;
    }
}