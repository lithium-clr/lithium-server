namespace Lithium.Server.Core.Semver;

public sealed class SemverRange(
    ISemverSatisfies[] comparators,
    bool and
) : ISemverSatisfies
{
    private static readonly SemverRange Wildcard = new([], true);

    public bool Satisfies(Semver semver)
    {
        if (and)
        {
            foreach (var comp in comparators)
                if (!comp.Satisfies(semver))
                    return false;

            return true;
        }

        foreach (var comp in comparators)
            if (comp.Satisfies(semver))
                return true;

        return false;
    }

    public override string ToString()
    {
        return string.Join(" || ", comparators.Select(c => c.ToString()));
    }

    public static SemverRange FromString(string str) => FromString(str, false);

    public static SemverRange FromString(string str, bool strict)
    {
        if (str is null)
            throw new ArgumentNullException(nameof(str), "String can't be null!");

        str = str.Trim();

        if (string.IsNullOrWhiteSpace(str) || str is "*")
            return Wildcard;

        var split = str.Split(["||"], StringSplitOptions.None);
        var comparators = new ISemverSatisfies[split.Length];

        for (var i = 0; i < split.Length; i++)
        {
            var subRange = split[i].Trim();

            if (subRange.Contains(" - "))
            {
                var range = subRange.Split([" - "], StringSplitOptions.None);
                if (range.Length is not 2) throw new ArgumentException("Range has an invalid number of arguments!");

                comparators[i] = new SemverRange([
                    new SemverComparator(SemverComparator.ComparisonType.GreaterThanOrEqual,
                        Semver.FromString(range[0], strict)),
                    new SemverComparator(SemverComparator.ComparisonType.LessThanOrEqual,
                        Semver.FromString(range[1], strict))
                ], true);
            }
            else if (subRange.StartsWith('~'))
            {
                var semver = Semver.FromString(subRange[1..], strict);

                if (semver.Minor > 0)
                {
                    comparators[i] = new SemverRange([
                        new SemverComparator(SemverComparator.ComparisonType.GreaterThanOrEqual, semver),
                        new SemverComparator(SemverComparator.ComparisonType.LessThan,
                            new Semver(semver.Major, semver.Minor + 1, 0))
                    ], true);
                }
                else
                {
                    comparators[i] = new SemverRange([
                        new SemverComparator(SemverComparator.ComparisonType.GreaterThanOrEqual, semver),
                        new SemverComparator(SemverComparator.ComparisonType.LessThan,
                            new Semver(semver.Major + 1, 0, 0))
                    ], true);
                }
            }
            else if (subRange.StartsWith('^'))
            {
                var semver = Semver.FromString(subRange[1..], strict);

                if (semver.Major > 0)
                {
                    comparators[i] = new SemverRange([
                        new SemverComparator(SemverComparator.ComparisonType.GreaterThanOrEqual, semver),
                        new SemverComparator(SemverComparator.ComparisonType.LessThan,
                            new Semver(semver.Major + 1, 0, 0))
                    ], true);
                }
                else if (semver.Minor > 0)
                {
                    comparators[i] = new SemverRange([
                        new SemverComparator(SemverComparator.ComparisonType.GreaterThanOrEqual, semver),
                        new SemverComparator(SemverComparator.ComparisonType.LessThan,
                            new Semver(0, semver.Minor + 1, 0))
                    ], true);
                }
                else
                {
                    comparators[i] = new SemverRange([
                        new SemverComparator(SemverComparator.ComparisonType.GreaterThanOrEqual, semver),
                        new SemverComparator(SemverComparator.ComparisonType.LessThan,
                            new Semver(0, 0, semver.Patch + 1))
                    ], true);
                }
            }
            else if (SemverComparator.HasAPrefix(subRange))
            {
                comparators[i] = SemverComparator.FromString(subRange);
            }
            else if (!subRange.Contains(' '))
            {
                var cleanRange = subRange.Replace("x", "0").Replace("*", "0");
                var semver = Semver.FromString(cleanRange, strict);

                switch (semver)
                {
                    case { Patch: 0, Minor: 0, Major: 0 }:
                        comparators[i] = new SemverComparator(SemverComparator.ComparisonType.GreaterThanOrEqual,
                            new Semver(0, 0, 0));
                        break;
                    case { Patch: 0, Minor: 0 }:
                        comparators[i] = new SemverRange([
                            new SemverComparator(SemverComparator.ComparisonType.GreaterThanOrEqual, semver),
                            new SemverComparator(SemverComparator.ComparisonType.LessThan,
                                new Semver(semver.Major + 1, 0, 0))
                        ], true);
                        break;
                    default:
                    {
                        if (semver.Patch is not 0)
                            throw new ArgumentException("Invalid X-Range! " + subRange);

                        comparators[i] = new SemverRange([
                            new SemverComparator(SemverComparator.ComparisonType.GreaterThanOrEqual, semver),
                            new SemverComparator(SemverComparator.ComparisonType.LessThan,
                                new Semver(semver.Major, semver.Minor + 1, 0))
                        ], true);
                        break;
                    }
                }
            }
            else
            {
                var comparatorStrings = subRange.Split(' ');
                var comparatorsAnd = new ISemverSatisfies[comparatorStrings.Length];

                for (var y = 0; y < comparatorStrings.Length; y++)
                    comparatorsAnd[y] = SemverComparator.FromString(comparatorStrings[y]);

                comparators[i] = new SemverRange(comparatorsAnd, true);
            }
        }

        return new SemverRange(comparators, false);
    }
}