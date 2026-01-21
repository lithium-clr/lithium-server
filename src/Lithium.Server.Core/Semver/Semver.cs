using Semver;

namespace Lithium.Server.Core.Semver;

public sealed record Semver : IComparable<Semver>
{
    private readonly SemVersion _version;

    public Semver(long major, long minor, long patch) : this(major, minor, patch, [], null)
    {
    }

    public Semver(long major, long minor, long patch, string[]? preRelease, string? build)
    {
        var preReleaseStr = preRelease is { Length: > 0 } ? string.Join(".", preRelease) : "";
        _version = SemVersion.ParsedFrom(major, minor, patch, preReleaseStr, build ?? "");
    }

    internal Semver(SemVersion version)
    {
        _version = version;
    }

    public long Major => (long)_version.Major;
    public long Minor => (long)_version.Minor;
    public long Patch => (long)_version.Patch;

    public string[] PreRelease => _version.PrereleaseIdentifiers.Select(x => x.Value).ToArray();
    public string Build => _version.Metadata;

    public bool Satisfies(SemverRange range)
    {
        return range.Satisfies(this);
    }

    public int CompareTo(Semver? other)
    {
        return other is null ? 1 : SemVersion.PrecedenceComparer.Compare(_version, other._version);
    }

    public override string ToString() => _version.ToString();

    public static Semver FromString(string str) => FromString(str, false);

    public static Semver FromString(string str, bool strict)
    {
        if (string.IsNullOrWhiteSpace(str))
            throw new ArgumentException("String is empty!", nameof(str));

        str = str.Trim();

        if (str.StartsWith("=") || str.StartsWith("v"))
            str = str[1..];

        if (str.StartsWith("=") || str.StartsWith("v"))
            str = str[1..];

        str = str.Trim();

        if (string.IsNullOrEmpty(str))
            throw new ArgumentException("String is empty!", nameof(str));

        try
        {
            string? build = null;

            if (str.Contains("+"))
            {
                var parts = str.Split(['+'], 2);

                str = parts[0];
                build = parts[1];

                ValidateBuild(build);
            }

            string[]? preRelease = null;

            if (str.Contains("-"))
            {
                var parts = str.Split(['-'], 2);

                str = parts[0];
                preRelease = parts[1].Split('.');

                ValidatePreRelease(preRelease);
            }

            if (string.IsNullOrEmpty(str) || (str[0] != '.' && str[^1] != '.'))
            {
                var segments = str.Split('.');

                if (segments.Length < 1)
                    throw new ArgumentException($"String doesn't match <major>.<minor>.<patch> ({str})");

                var major = long.Parse(segments[0]);
                if (major < 0) throw new ArgumentException($"Major must be a non-negative integers ({str})");

                if (!strict && segments.Length == 1)
                    return new Semver(major, 0, 0, preRelease, build);

                if (segments.Length < 2)
                    throw new ArgumentException($"String doesn't match <major>.<minor>.<patch> ({str})");

                var minor = long.Parse(segments[1]);
                if (minor < 0) throw new ArgumentException($"Minor must be a non-negative integers ({str})");

                if (!strict && segments.Length == 2)
                    return new Semver(major, minor, 0, preRelease, build);

                if (segments.Length != 3)
                    throw new ArgumentException($"String doesn't match <major>.<minor>.<patch> ({str})");

                var patchStr = segments[2];

                if (!strict && preRelease == null)
                {
                    for (var i = 0; i < patchStr.Length; i++)
                    {
                        if (char.IsDigit(patchStr[i])) continue;

                        var pre = patchStr[i..];
                        patchStr = patchStr[..i];

                        if (!string.IsNullOrWhiteSpace(pre))
                        {
                            preRelease = pre.Split('.');
                            ValidatePreRelease(preRelease);
                        }

                        break;
                    }
                }

                var patch = long.Parse(patchStr);

                return patch < 0
                    ? throw new ArgumentException($"Patch must be a non-negative integers ({str})")
                    : new Semver(major, minor, patch, preRelease, build);
            }

            throw new ArgumentException($"Failed to parse digits ({str})");
        }
        catch (FormatException)
        {
            throw new ArgumentException($"Failed to parse digits ({str})");
        }
    }

    private static void ValidateBuild(string? build)
    {
        if (build is null) return;

        if (build.Length is 0 || !IsAlphaNumericHyphenString(build))
            throw new ArgumentException($"Build must only be alphanumeric ({build})");
    }

    private static void ValidatePreRelease(string[]? preRelease)
    {
        if (preRelease is null) return;

        foreach (var part in preRelease)
        {
            if (part.Length is not 0 && IsAlphaNumericHyphenString(part)) continue;

            var arrStr = "[" + string.Join(", ", preRelease) + "]";
            throw new ArgumentException($"Pre-release must only be alphanumeric ({arrStr})");
        }
    }

    private static bool IsAlphaNumericHyphenString(string str)
    {
        foreach (var c in str)
        {
            if (!char.IsLetterOrDigit(c) && c != '-')
                return false;
        }

        return true;
    }

    public static implicit operator SemVersion(Semver s) => s._version;
}