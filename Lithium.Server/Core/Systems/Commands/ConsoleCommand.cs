using System.Reflection;

namespace Lithium.Server.Core.Systems.Commands;

public sealed class ConsoleCommand
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required MethodInfo Method { get; init; }
    public required Type? DeclaringType { get; init; }
}