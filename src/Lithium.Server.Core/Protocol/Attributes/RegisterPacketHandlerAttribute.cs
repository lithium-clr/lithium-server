namespace Lithium.Server.Core.Protocol.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RegisterPacketHandlerAttribute(Type routerType) : Attribute
{
    public Type RouterType { get; } = routerType;
}
