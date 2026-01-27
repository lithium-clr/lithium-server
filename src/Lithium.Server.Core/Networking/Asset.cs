using Lithium.Server.Core.Networking.Protocol;
using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking;

public sealed record Asset : PacketObject
{
    [PacketProperty(FixedIndex = 0, FixedSize = 64)]
    public string Hash { get; init; }

    [PacketProperty]
    public string Name { get; init; }
}