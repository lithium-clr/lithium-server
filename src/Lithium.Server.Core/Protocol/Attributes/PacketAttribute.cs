namespace Lithium.Server.Core.Protocol.Attributes;

/// <summary>
/// Marks a struct or class as a network packet.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class PacketAttribute(int id) : Attribute
{
    public int Id { get; } = id;
    public bool IsCompressed { get; set; }
    public int VariableBlockStart { get; set; }
    public int MaxSize { get; set; }
}