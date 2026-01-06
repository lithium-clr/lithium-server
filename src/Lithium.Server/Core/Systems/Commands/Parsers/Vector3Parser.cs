using System.Globalization;
using System.Numerics;

namespace Lithium.Server.Core.Systems.Commands.Parsers;

public sealed class Vector3Parser : ICommandArgumentParser<Vector3>
{
    public Type TargetType => typeof(Vector3);

    public Vector3 Parse(string value)
    {
        var parts = value.Split(',');

        if (parts.Length is not 3)
            throw new FormatException("Expected format: x,y,z");

        return new Vector3(
            float.Parse(parts[0], CultureInfo.InvariantCulture),
            float.Parse(parts[1], CultureInfo.InvariantCulture),
            float.Parse(parts[2], CultureInfo.InvariantCulture)
        );
    }

    object ICommandArgumentParser.Parse(string value)
        => Parse(value);
}