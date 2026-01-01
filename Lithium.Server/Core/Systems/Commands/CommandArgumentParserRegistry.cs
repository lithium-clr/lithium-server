using System.Collections.Concurrent;

namespace Lithium.Server.Core.Systems.Commands;

public sealed class CommandArgumentParserRegistry(ILogger<CommandArgumentParserRegistry> logger)
{
    private readonly ConcurrentDictionary<Type, ICommandArgumentParser> _parsers = new();

    public void Register(ICommandArgumentParser parser)
    {
        _parsers[Normalize(parser.TargetType)] = parser;
        logger.LogInformation($"Register parser for type: {parser.TargetType}");
    }

    public bool TryGet(Type type, out ICommandArgumentParser parser)
        => _parsers.TryGetValue(Normalize(type), out parser!);

    private static Type Normalize(Type type)
        => Nullable.GetUnderlyingType(type) ?? type;
}