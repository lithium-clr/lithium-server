using System;

namespace Lithium.Core.Semver;

public class SemverComparator : ISemverSatisfies
{
    public enum ComparisonType
    {
        GTE,
        GT,
        LTE,
        LT,
        EQUAL
    }

    private readonly ComparisonType _comparisonType;
    private readonly Semver _compareTo;

    public SemverComparator(ComparisonType comparisonType, Semver compareTo)
    {
        _comparisonType = comparisonType;
        _compareTo = compareTo;
    }

    public bool Satisfies(Semver semver)
    {
        int c = _compareTo.CompareTo(semver);
        return _comparisonType switch
        {
            ComparisonType.GTE => c <= 0, // threshold <= input
            ComparisonType.GT => c < 0,   // threshold < input
            ComparisonType.LTE => c >= 0, // threshold >= input
            ComparisonType.LT => c > 0,   // threshold > input
            ComparisonType.EQUAL => c == 0,
            _ => false
        };
    }

    public override string ToString()
    {
        return GetPrefix(_comparisonType) + _compareTo;
    }

    public static SemverComparator FromString(string str)
    {
        if (str == null) throw new ArgumentNullException(nameof(str), "String can't be null!");
        str = str.Trim();
        if (str.Length == 0) throw new ArgumentException("String is empty!");

        // Iterate in specific order to match prefixes correctly (e.g. >= before >)
        // Java order: GTE, GT, LTE, LT, EQUAL
        var types = new[] { ComparisonType.GTE, ComparisonType.GT, ComparisonType.LTE, ComparisonType.LT, ComparisonType.EQUAL };

        foreach (var type in types)
        {
            string prefix = GetPrefix(type);
            if (str.StartsWith(prefix))
            {
                var verStr = str.Substring(prefix.Length);
                Semver semver = Semver.FromString(verStr);
                return new SemverComparator(type, semver);
            }
        }

        throw new ArgumentException("Invalid comparator type! " + str);
    }
    
    public static bool HasAPrefix(string range)
    {
        var types = new[] { ComparisonType.GTE, ComparisonType.GT, ComparisonType.LTE, ComparisonType.LT, ComparisonType.EQUAL };
        foreach (var type in types)
        {
            if (range.StartsWith(GetPrefix(type))) return true;
        }
        return false;
    }

    private static string GetPrefix(ComparisonType type) => type switch
    {
        ComparisonType.GTE => ">=",
        ComparisonType.GT => ">",
        ComparisonType.LTE => "<=",
        ComparisonType.LT => "<",
        ComparisonType.EQUAL => "=",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
