using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Semver;

namespace Lithium.Core.Semver;

public sealed record Semver : IComparable<Semver>
{
    private readonly SemVersion _version;

    public Semver(long major, long minor, long patch) : this(major, minor, patch, Array.Empty<string>(), null)
    {
    }

    public Semver(long major, long minor, long patch, string[]? preRelease, string? build)
    {
        var preReleaseStr = preRelease != null && preRelease.Length > 0 ? string.Join(".", preRelease) : "";
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
        if (other is null) return 1;
        return SemVersion.PrecedenceComparer.Compare(_version, other._version);
    }

    public override string ToString()
    {
        return _version.ToString();
    }
    
    public static Semver FromString(string str) => FromString(str, false);

    public static Semver FromString(string str, bool strict)
    {
        if (string.IsNullOrWhiteSpace(str))
            throw new ArgumentException("String is empty!", nameof(str));

        str = str.Trim();
        
        if (str.StartsWith("=") || str.StartsWith("v"))
            str = str.Substring(1);
        if (str.StartsWith("=") || str.StartsWith("v"))
            str = str.Substring(1);
            
        str = str.Trim();
        if (string.IsNullOrEmpty(str))
             throw new ArgumentException("String is empty!", nameof(str));

        try 
        {
            string? build = null;
            if (str.Contains("+"))
            {
                var parts = str.Split(new[] { '+' }, 2);
                str = parts[0];
                build = parts[1];
                ValidateBuild(build);
            }

            string[]? preRelease = null;
            if (str.Contains("-"))
            {
                var parts = str.Split(new[] { '-' }, 2);
                str = parts[0];
                preRelease = parts[1].Split('.');
                ValidatePreRelease(preRelease);
            }
            
            if (string.IsNullOrEmpty(str) || (str[0] != '.' && str[^1] != '.'))
            {
                var segments = str.Split('.');
                if (segments.Length < 1) throw new ArgumentException($"String doesn't match <major>.<minor>.<patch> ({str})");
                
                long major = long.Parse(segments[0]);
                if (major < 0) throw new ArgumentException($"Major must be a non-negative integers ({str})");

                if (!strict && segments.Length == 1)
                    return new Semver(major, 0, 0, preRelease, build);
                    
                if (segments.Length < 2) throw new ArgumentException($"String doesn't match <major>.<minor>.<patch> ({str})");
                
                long minor = long.Parse(segments[1]);
                if (minor < 0) throw new ArgumentException($"Minor must be a non-negative integers ({str})");

                if (!strict && segments.Length == 2)
                     return new Semver(major, minor, 0, preRelease, build);

                if (segments.Length != 3) throw new ArgumentException($"String doesn't match <major>.<minor>.<patch> ({str})");

                string patchStr = segments[2];
                if (!strict && preRelease == null)
                {
                    for (int i = 0; i < patchStr.Length; i++)
                    {
                        if (!char.IsDigit(patchStr[i]))
                        {
                            var pre = patchStr.Substring(i);
                            patchStr = patchStr.Substring(0, i);
                            if (!string.IsNullOrWhiteSpace(pre))
                            {
                                preRelease = pre.Split('.');
                                ValidatePreRelease(preRelease);
                            }
                            break;
                        }
                    }
                }
                
                long patch = long.Parse(patchStr);
                if (patch < 0) throw new ArgumentException($"Patch must be a non-negative integers ({str})");

                return new Semver(major, minor, patch, preRelease, build);
            }
            else
            {
                 throw new ArgumentException($"Failed to parse digits ({str})");
            }
        }
        catch (FormatException)
        {
             throw new ArgumentException($"Failed to parse digits ({str})");
        }
    }
    
    private static void ValidateBuild(string? build)
    {
        if (build != null)
        {
            if (build.Length == 0 || !IsAlphaNumericHyphenString(build))
            {
                throw new ArgumentException($"Build must only be alphanumeric ({build})");
            }
        }
    }

    private static void ValidatePreRelease(string[]? preRelease)
    {
        if (preRelease != null)
        {
            foreach (var part in preRelease)
            {
                if (part.Length == 0 || !IsAlphaNumericHyphenString(part))
                {
                    var arrStr = "[" + string.Join(", ", preRelease) + "]";
                    throw new ArgumentException($"Pre-release must only be alphanumeric ({arrStr})");
                }
            }
        }
    }

    private static bool IsAlphaNumericHyphenString(string str)
    {
        foreach (char c in str)
        {
            if (!char.IsLetterOrDigit(c) && c != '-') return false;
        }
        return true;
    }
    
    public static implicit operator SemVersion(Semver s) => s._version;
}