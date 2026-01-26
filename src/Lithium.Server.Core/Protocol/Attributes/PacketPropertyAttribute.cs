namespace Lithium.Server.Core.Protocol.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class PacketPropertyAttribute : Attribute
{
    public int FixedIndex { get; set; }
    public int FixedSize { get; set; }
    public int BitIndex { get; set; }
    public int OffsetIndex { get; set; }
}