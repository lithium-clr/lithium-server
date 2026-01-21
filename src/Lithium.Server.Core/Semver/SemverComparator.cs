namespace Lithium.Server.Core.Semver;

public sealed class SemverComparator(
    SemverComparator.ComparisonType comparisonType,
    Semver compareTo
) : ISemverSatisfies
{
    public enum ComparisonType
    {
        GreaterThanOrEqual,
        GreaterThan,
        LessThanOrEqual,
        LessThan,
        Equal
    }

    public bool Satisfies(Semver semver)
    {
        var c = compareTo.CompareTo(semver);

        return comparisonType switch
        {
            ComparisonType.GreaterThanOrEqual => c <= 0,
            ComparisonType.GreaterThan => c < 0,
            ComparisonType.LessThanOrEqual => c >= 0,
            ComparisonType.LessThan => c > 0,
            ComparisonType.Equal => c == 0,
            _ => false
        };
    }

    public override string ToString()
    {
        return GetPrefix(comparisonType) + compareTo;
    }

    public static SemverComparator FromString(string str)
    {
        if (str is null)
            throw new ArgumentNullException(nameof(str), "String can't be null!");

        str = str.Trim();

        if (str.Length is 0)
            throw new ArgumentException("String is empty!");

        var types = new[]
        {
            ComparisonType.GreaterThanOrEqual, ComparisonType.GreaterThan, ComparisonType.LessThanOrEqual,
            ComparisonType.LessThan, ComparisonType.Equal
        };

        foreach (var type in types)
        {
            var prefix = GetPrefix(type);

            if (!str.StartsWith(prefix)) continue;

            var verStr = str[prefix.Length..];
            var semver = Semver.FromString(verStr);

            return new SemverComparator(type, semver);
        }

        throw new ArgumentException("Invalid comparator type! " + str);
    }

    public static bool HasAPrefix(string range)
    {
        var types = new[]
        {
            ComparisonType.GreaterThanOrEqual, ComparisonType.GreaterThan, ComparisonType.LessThanOrEqual,
            ComparisonType.LessThan, ComparisonType.Equal
        };

        foreach (var type in types)
        {
            if (range.StartsWith(GetPrefix(type)))
                return true;
        }

        return false;
    }

    private static string GetPrefix(ComparisonType type) => type switch
    {
        ComparisonType.GreaterThanOrEqual => ">=",
        ComparisonType.GreaterThan => ">",
        ComparisonType.LessThanOrEqual => "<=",
        ComparisonType.LessThan => "<",
        ComparisonType.Equal => "=",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}