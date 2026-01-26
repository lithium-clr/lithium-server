namespace Lithium.Server.Core.Protocol.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class PacketPropertyAttribute : Attribute
{
    public int FixedIndex { get; set; } = -1;
    public int FixedSize { get; set; } = -1;
    public int BitIndex { get; set; } = -1;
    public int OffsetIndex { get; set; } = -1;
}
