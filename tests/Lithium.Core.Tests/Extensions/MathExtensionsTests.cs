using FluentAssertions;
using Lithium.Core.Extensions;

namespace Lithium.Core.Tests.Extensions;

public class MathExtensionsTests
{
    [Fact]
    public void Clamp_ShouldReturnMin_WhenValueIsLessThanMin()
    {
        10.Clamp(20, 30).Should().Be(20);
    }

    [Fact]
    public void Clamp_ShouldReturnMax_WhenValueIsGreaterThanMax()
    {
        40.Clamp(20, 30).Should().Be(30);
    }

    [Fact]
    public void Clamp_ShouldReturnValue_WhenValueIsWithinRange()
    {
        25.Clamp(20, 30).Should().Be(25);
    }

    [Fact]
    public void Lerp_ShouldInterpolateCorrectly()
    {
        0f.Lerp(100f, 0.5f).Should().Be(50f);
        0f.Lerp(100f, 0f).Should().Be(0f);
        0f.Lerp(100f, 1f).Should().Be(100f);
    }
    
    [Fact]
    public void Lerp_ShouldClampT()
    {
        0f.Lerp(100f, 1.5f).Should().Be(100f);
        0f.Lerp(100f, -0.5f).Should().Be(0f);
    }

    [Fact]
    public void LerpUnclamped_ShouldExtrapolate()
    {
        0f.LerpUnclamped(100f, 1.5f).Should().Be(150f);
    }

    [Fact]
    public void PercentOf_ShouldCalculatePercentage()
    {
        50.PercentOf(200).Should().Be(25.0);
    }
    
    [Fact]
    public void Remap_ShouldMapRanges()
    {
        5f.Remap(0f, 10f, 0f, 100f).Should().Be(50f);
    }
    
    [Fact]
    public void WrapAngle_ShouldKeepAngleWithinPi()
    {
        float pi = MathF.PI;
        (pi + 0.1f).WrapAngle().Should().BeApproximately(-pi + 0.1f, 0.0001f);
        (3 * pi).WrapAngle().Should().BeApproximately(pi, 0.0001f);
    }
}
