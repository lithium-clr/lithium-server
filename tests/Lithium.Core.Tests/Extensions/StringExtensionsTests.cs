using FluentAssertions;
using Lithium.Core.Extensions;

namespace Lithium.Core.Tests.Extensions;

public class StringExtensionsTests
{
    [Fact]
    public void IsDigits_ShouldReturnTrueForDigits()
    {
        "123456".IsDigits().Should().BeTrue();
    }
    
    [Fact]
    public void IsDigits_ShouldReturnFalseForNonDigits()
    {
        "123a456".IsDigits().Should().BeFalse();
        "-123".IsDigits().Should().BeFalse();
        "12.3".IsDigits().Should().BeFalse();
    }
    
    [Fact]
    public void Capitalize_ShouldCapitalizeWords()
    {
        "hello world".Capitalize().Should().Be("Hello World");
        "hello-world".Capitalize('-').Should().Be("Hello-World");
    }
    
    [Fact]
    public void Humanize_ShouldFormatTimeSpan()
    {
        var ts = new TimeSpan(2, 5, 30, 0);
        ts.Humanize().Should().Be("2d 5h 30m");
    }
    
    [Fact]
    public void LevenshteinDistance_ShouldComputeCorrectly()
    {
        "kitten".LevenshteinDistance("sitting").Should().Be(3);
        "flaw".LevenshteinDistance("lawn").Should().Be(2);
    }
    
    [Fact]
    public void FuzzyDistance_ShouldComputeCorrectly()
    {
        // "abc" vs "axbycz" -> 3 matches.
        // "abc" vs "abc" -> 3 + 2 + 2 = 7 (bonus for consecutive)
        
        "abc".FuzzyDistance("abc").Should().BeGreaterThan("abc".FuzzyDistance("axbycz"));
    }

    [Fact]
    public void IsAlphaNumericHyphen_ShouldReturnTrueForValidStrings()
    {
        "abc-123".IsAlphaNumericHyphen().Should().BeTrue();
        "Hello-World".IsAlphaNumericHyphen().Should().BeTrue();
    }

    [Fact]
    public void IsAlphaNumericHyphen_ShouldReturnFalseForInvalidStrings()
    {
        "abc_123".IsAlphaNumericHyphen().Should().BeFalse();
        "hello world".IsAlphaNumericHyphen().Should().BeFalse();
        "".IsAlphaNumericHyphen().Should().BeFalse();
    }
}
