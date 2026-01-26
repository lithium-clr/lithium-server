namespace Lithium.Server.Core.Protocol.Attributes;

/// <summary>
/// Marks a struct or class as a network packet.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class PacketAttribute : Attribute
{
    public int Id { get; init; }
    public bool IsCompressed { get; init; }
    public int VariableBlockStart { get; init; }
    public int MaxSize { get; init; } = 16384; // Default to 16KB

    public PacketAttribute() { }

    public PacketAttribute(int id)
    {
        Id = id;
    }
}