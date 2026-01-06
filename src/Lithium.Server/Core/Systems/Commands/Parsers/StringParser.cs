namespace Lithium.Server.Core.Systems.Commands.Parsers;

public sealed class StringParser : ICommandArgumentParser<string>
{
    public Type TargetType => typeof(string);

    public string Parse(string value) => value;

    object ICommandArgumentParser.Parse(string value)
        => value;
}