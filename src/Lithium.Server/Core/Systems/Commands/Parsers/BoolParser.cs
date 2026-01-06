namespace Lithium.Server.Core.Systems.Commands.Parsers;

public sealed class BoolParser : ICommandArgumentParser<bool>
{
    public Type TargetType => typeof(bool);

    public bool Parse(string value)
        => bool.Parse(value);

    object ICommandArgumentParser.Parse(string value)
        => Parse(value);
}