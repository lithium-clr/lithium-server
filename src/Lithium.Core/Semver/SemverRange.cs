using System;
using System.Linq;

namespace Lithium.Core.Semver;

public class SemverRange : ISemverSatisfies
{
    public static readonly SemverRange WILDCARD = new SemverRange(Array.Empty<ISemverSatisfies>(), true);

    private readonly ISemverSatisfies[] _comparators;
    private readonly bool _and;

    public SemverRange(ISemverSatisfies[] comparators, bool and)
    {
        _comparators = comparators;
        _and = and;
    }

    public bool Satisfies(Semver semver)
    {
        if (_and)
        {
            foreach (var comp in _comparators)
            {
                if (!comp.Satisfies(semver)) return false;
            }
            return true;
        }
        else
        {
            foreach (var comp in _comparators)
            {
                if (comp.Satisfies(semver)) return true;
            }
            return false;
        }
    }

    public override string ToString()
    {
        return string.Join(" || ", _comparators.Select(c => c.ToString()));
    }
    
    public static SemverRange FromString(string str) => FromString(str, false);

    public static SemverRange FromString(string str, bool strict)
    {
        if (str == null) throw new ArgumentNullException(nameof(str), "String can't be null!");
        str = str.Trim();
        
        if (!string.IsNullOrWhiteSpace(str) && str != "*")
        {
            var split = str.Split(new[] { "||" }, StringSplitOptions.None);
            var comparators = new ISemverSatisfies[split.Length];

            for (int i = 0; i < split.Length; i++)
            {
                string subRange = split[i].Trim();
                
                if (subRange.Contains(" - "))
                {
                    var range = subRange.Split(new[] { " - " }, StringSplitOptions.None);
                    if (range.Length != 2) throw new ArgumentException("Range has an invalid number of arguments!");
                    
                    comparators[i] = new SemverRange(new ISemverSatisfies[] {
                        new SemverComparator(SemverComparator.ComparisonType.GTE, Semver.FromString(range[0], strict)),
                        new SemverComparator(SemverComparator.ComparisonType.LTE, Semver.FromString(range[1], strict))
                    }, true);
                }
                else if (subRange.StartsWith("~"))
                {
                    Semver semver = Semver.FromString(subRange.Substring(1), strict);
                    if (semver.Minor > 0)
                    {
                        comparators[i] = new SemverRange(new ISemverSatisfies[] {
                            new SemverComparator(SemverComparator.ComparisonType.GTE, semver),
                            new SemverComparator(SemverComparator.ComparisonType.LT, new Semver(semver.Major, semver.Minor + 1, 0))
                        }, true);
                    }
                    else
                    {
                        comparators[i] = new SemverRange(new ISemverSatisfies[] {
                             new SemverComparator(SemverComparator.ComparisonType.GTE, semver),
                             new SemverComparator(SemverComparator.ComparisonType.LT, new Semver(semver.Major + 1, 0, 0))
                        }, true);
                    }
                }
                else if (subRange.StartsWith("^"))
                {
                     Semver semver = Semver.FromString(subRange.Substring(1), strict);
                     if (semver.Major > 0)
                     {
                         comparators[i] = new SemverRange(new ISemverSatisfies[] {
                             new SemverComparator(SemverComparator.ComparisonType.GTE, semver),
                             new SemverComparator(SemverComparator.ComparisonType.LT, new Semver(semver.Major + 1, 0, 0))
                         }, true);
                     }
                     else if (semver.Minor > 0)
                     {
                         comparators[i] = new SemverRange(new ISemverSatisfies[] {
                             new SemverComparator(SemverComparator.ComparisonType.GTE, semver),
                             new SemverComparator(SemverComparator.ComparisonType.LT, new Semver(0, semver.Minor + 1, 0))
                         }, true);
                     }
                     else
                     {
                         comparators[i] = new SemverRange(new ISemverSatisfies[] {
                             new SemverComparator(SemverComparator.ComparisonType.GTE, semver),
                             new SemverComparator(SemverComparator.ComparisonType.LT, new Semver(0, 0, semver.Patch + 1))
                         }, true);
                     }
                }
                else if (SemverComparator.HasAPrefix(subRange))
                {
                    comparators[i] = SemverComparator.FromString(subRange);
                }
                else if (!subRange.Contains(" "))
                {
                    string cleanRange = subRange.Replace("x", "0").Replace("*", "0");
                    Semver semver = Semver.FromString(cleanRange, strict);
                    
                    if (semver.Patch == 0 && semver.Minor == 0 && semver.Major == 0)
                    {
                        comparators[i] = new SemverComparator(SemverComparator.ComparisonType.GTE, new Semver(0, 0, 0));
                    }
                    else if (semver.Patch == 0 && semver.Minor == 0)
                    {
                         comparators[i] = new SemverRange(new ISemverSatisfies[] {
                             new SemverComparator(SemverComparator.ComparisonType.GTE, semver),
                             new SemverComparator(SemverComparator.ComparisonType.LT, new Semver(semver.Major + 1, 0, 0))
                         }, true);
                    }
                    else
                    {
                        if (semver.Patch != 0)
                        {
                            throw new ArgumentException("Invalid X-Range! " + subRange);
                        }
                         comparators[i] = new SemverRange(new ISemverSatisfies[] {
                             new SemverComparator(SemverComparator.ComparisonType.GTE, semver),
                             new SemverComparator(SemverComparator.ComparisonType.LT, new Semver(semver.Major, semver.Minor + 1, 0))
                         }, true);
                    }
                }
                else
                {
                    var comparatorStrings = subRange.Split(' ');
                    var comparatorsAnd = new ISemverSatisfies[comparatorStrings.Length];
                    for (int y = 0; y < comparatorStrings.Length; y++)
                    {
                        comparatorsAnd[y] = SemverComparator.FromString(comparatorStrings[y]);
                    }
                    comparators[i] = new SemverRange(comparatorsAnd, true);
                }
            }
            
            return new SemverRange(comparators, false);
        }
        else
        {
            return WILDCARD;
        }
    }
}
