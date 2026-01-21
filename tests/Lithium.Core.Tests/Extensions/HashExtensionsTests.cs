using FluentAssertions;
using Lithium.Core.Extensions;

namespace Lithium.Core.Tests.Extensions;

public class HashExtensionsTests
{
    [Fact]
    public void Hash_ShouldBeDeterministic()
    {
        long h1 = HashExtensions.Hash(12345);
        long h2 = HashExtensions.Hash(12345);
        h1.Should().Be(h2);
    }

    [Fact]
    public void Hash_ShouldChangeAvalanche()
    {
        long h1 = HashExtensions.Hash(12345);
        long h2 = HashExtensions.Hash(12346);
        h1.Should().NotBe(h2);
    }

    [Fact]
    public void RandomDouble_ShouldBeNormalized()
    {
        double d = HashExtensions.RandomDouble(123);
        d.Should().BeGreaterThanOrEqualTo(0.0);
        d.Should().BeLessThan(1.0);
    }
}
