using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 80, IsCompressed = true, VariableBlockStart = 6, MaxSize = 1677721600)]
public sealed class UpdateAudioCategoriesPacket : Packet
{
    // Java: type (fixed, offset 1)
    [PacketProperty(FixedIndex = 0)]
    public UpdateType Type { get; init; } = UpdateType.Init;

    // Java: maxId (fixed, offset 2)
    [PacketProperty(FixedIndex = 1)]
    public int MaxId { get; init; }

    // Java: categories (nullable, bit 0), no OffsetIndex (sequential)
    [PacketProperty(BitIndex = 0)]
    public Dictionary<int, AudioCategory>? Categories { get; init; }
}