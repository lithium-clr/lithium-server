namespace Lithium.Server.Core.Commands;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ConsoleCommandAttribute(string name, string? description = null) : Attribute
{
    public readonly string Name = name.ToLowerInvariant();
    public readonly string? Description = description;
}