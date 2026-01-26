using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 32, IsCompressed = false, VariableBlockStart = 4, MaxSize = 4)]
public sealed class ViewRadiusPacket : Packet
{
    // Java: value (fixed, offset 0)
    [PacketProperty(FixedIndex = 0)]
    public int Value { get; set; }

    public ViewRadiusPacket() { }

    public ViewRadiusPacket(int value)
    {
        Value = value;
    }
}