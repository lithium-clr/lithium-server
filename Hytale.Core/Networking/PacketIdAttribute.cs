namespace Hytale.Core.Networking;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class PacketIdAttribute(ushort id) : Attribute
{
    public readonly ushort Id = id;
}