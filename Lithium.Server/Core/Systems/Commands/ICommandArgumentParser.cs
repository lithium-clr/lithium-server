namespace Lithium.Server.Core.Systems.Commands;

public interface ICommandArgumentParser
{
    Type TargetType { get; }
    object Parse(string value);
}

public interface ICommandArgumentParser<out T> : ICommandArgumentParser
{
    new T Parse(string value);
}