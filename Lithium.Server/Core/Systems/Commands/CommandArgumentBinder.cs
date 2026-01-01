using System.Reflection;

namespace Lithium.Server.Core.Systems.Commands;

public sealed class CommandArgumentBinder(CommandArgumentParserRegistry parsers)
{
    public object[] Bind(ParameterInfo[] parameters, string[] args)
    {
        if (args.Length != parameters.Length)
        {
            Console.WriteLine("Invalid arguments count.");
            return [];
        }

        var result = new object[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var targetType = parameters[i].ParameterType;
            Console.WriteLine("Target: " + targetType);

            if (parsers.TryGet(targetType, out var parser))
            {
                result[i] = parser.Parse(args[i]);
                continue;
            }
            else
            {
                throw new Exception($"Failed to parse: {targetType}");
            }

            // result[i] = ConvertFallback(args[i], targetType);
        }

        return result;
    }

    // private static object ConvertFallback(string value, Type targetType)
    // {
    //     var nullable = Nullable.GetUnderlyingType(targetType);
    //
    //     if (nullable is not null)
    //     {
    //         if (string.Equals(value, "null", StringComparison.OrdinalIgnoreCase))
    //             return null!;
    //
    //         targetType = nullable;
    //     }
    //
    //     return targetType.IsEnum
    //         ? Enum.Parse(targetType, value, true)
    //         : Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    // }
}