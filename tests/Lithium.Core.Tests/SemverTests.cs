using System;
using FluentAssertions;
using Lithium.Core.Semver;
using Xunit;

namespace Lithium.Core.Tests;

public class SemverTests
{
    [Theory]
    [InlineData("1.0.0", 1, 0, 0)]
    [InlineData("1.0", 1, 0, 0)]
    [InlineData("1", 1, 0, 0)]
    [InlineData("v1.2.3", 1, 2, 3)]
    [InlineData("=1.2.3", 1, 2, 3)]
    [InlineData("1.2.3-beta.1", 1, 2, 3)]
    [InlineData("1.2.3+build1", 1, 2, 3)]
    [InlineData("1.0.0beta", 1, 0, 0)] // Loose parsing
    public void FromString_ParsesCorrectly(string input, long major, long minor, long patch)
    {
        var ver = Lithium.Core.Semver.Semver.FromString(input);
        ver.Major.Should().Be(major);
        ver.Minor.Should().Be(minor);
        ver.Patch.Should().Be(patch);
    }
    
    [Fact]
    public void FromString_Strict_ThrowsOnPartial()
    {
        Assert.Throws<ArgumentException>(() => Lithium.Core.Semver.Semver.FromString("1", true));
        Assert.Throws<ArgumentException>(() => Lithium.Core.Semver.Semver.FromString("1.2", true));
    }

    [Theory]
    [InlineData("1.0.0", ">=1.0.0", true)]
    [InlineData("1.0.0", ">1.0.0", false)]
    [InlineData("1.2.3", "~1.2.0", true)] // >=1.2.0 <1.3.0
    [InlineData("1.3.0", "~1.2.0", false)]
    [InlineData("1.2.3", "^1.2.0", true)] // >=1.2.0 <2.0.0
    [InlineData("2.0.0", "^1.2.0", false)]
    [InlineData("0.2.3", "^0.2.0", true)] // >=0.2.0 <0.3.0
    [InlineData("0.3.0", "^0.2.0", false)]
    [InlineData("1.2.3", "1.2.x", true)]
    [InlineData("1.3.0", "1.2.x", false)]
    [InlineData("1.2.3", "*", true)]
    [InlineData("1.2.3", ">=1.0.0 || <0.5.0", true)]
    [InlineData("0.1.0", ">=1.0.0 || <0.5.0", true)]
    [InlineData("0.8.0", ">=1.0.0 || <0.5.0", false)]
    [InlineData("1.2.3", "=1.2.3", true)]
    public void Satisfies_Works(string version, string range, bool expected)
    {
        var v = Lithium.Core.Semver.Semver.FromString(version);
        var r = SemverRange.FromString(range);
        v.Satisfies(r).Should().Be(expected);
    }

    [Fact]
    public void Range_ExplicitVersion_RequiresEqualSign()
    {
        // 1.2.3 as range throws because it's interpreted as X-Range but patch!=0
        Assert.Throws<ArgumentException>(() => SemverRange.FromString("1.2.3"));
    }
}
