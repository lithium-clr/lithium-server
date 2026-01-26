namespace Lithium.Server.Core.Networking.Protocol.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RegisterPacketHandlerAttribute(Type routerType) : Attribute
{
    public Type RouterType { get; } = routerType;
}
