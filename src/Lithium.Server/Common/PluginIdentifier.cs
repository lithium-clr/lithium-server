namespace Lithium.Server.Common;

public sealed record PluginIdentifier(string Group, string Name)
{
    public static PluginIdentifier FromString(string str)
    {
        var parts = str.Split(':');

        return parts.Length is not 2
            ? throw new ArgumentException("Invalid plugin identifier")
            : new PluginIdentifier(parts[0], parts[1]);
    }

    public override string ToString()
    {
        return Group + ':' + Name;
    }
}