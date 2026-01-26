using Lithium.Server.Core.Networking.Protocol;
using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking;

public sealed record Asset(string Hash, string Name) : PacketObject
{
    [PacketProperty(FixedIndex = 0, FixedSize = 64)]
    public string Hash { get; set; } = Hash;

    [PacketProperty(OffsetIndex = 0)]
    public string Name { get; set; } = Name;
}