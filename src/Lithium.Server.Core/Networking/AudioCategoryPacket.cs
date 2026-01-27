using Lithium.Server.Core.Networking.Protocol;
using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking;

public sealed record AudioCategoryPacket : PacketObject
{
    [PacketProperty(FixedIndex = 0)]
    public float Volume { get; init; }

    [PacketProperty(BitIndex = 0)]
    public string? Id { get; init; }
}