using System.Collections.Concurrent;
using System.Reflection;

namespace Lithium.Server.Core.Systems.Commands;

public sealed class CommandArgumentParserRegistry(ILogger<CommandArgumentParserRegistry> logger)
{
    private readonly ConcurrentDictionary<Type, ICommandArgumentParser> _parsers = new();

    public void RegisterAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes().Where(x =>
                     x is { IsAbstract: false, IsInterface: false } &&
                     x.IsAssignableTo(typeof(ICommandArgumentParser))))
        {
            if (Activator.CreateInstance(type) is not ICommandArgumentParser parser)
                throw new Exception($"Failed to register parser for type: {type}");

            Register(parser);
        }
    }

    private void Register(ICommandArgumentParser parser)
    {
        _parsers[Normalize(parser.TargetType)] = parser;
        logger.LogInformation($"Register parser for type: {parser.TargetType}");
    }

    public bool TryGet(Type type, out ICommandArgumentParser parser)
        => _parsers.TryGetValue(Normalize(type), out parser!);

    private static Type Normalize(Type type)
        => Nullable.GetUnderlyingType(type) ?? type;
}