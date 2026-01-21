using FluentAssertions;
using Lithium.Core.Extensions;

namespace Lithium.Core.Tests.Extensions;

public class ChunkExtensionsTests
{
    [Fact]
    public void ToChunkCoordinate_ShouldShiftCorrectly()
    {
        32.ToChunkCoordinate().Should().Be(1);
        63.ToChunkCoordinate().Should().Be(1);
        0.ToChunkCoordinate().Should().Be(0);
        (-32).ToChunkCoordinate().Should().Be(-1);
    }

    [Fact]
    public void ToLocalCoordinate_ShouldMaskCorrectly()
    {
        32.ToLocalCoordinate().Should().Be(0);
        33.ToLocalCoordinate().Should().Be(1);
        (-1).ToLocalCoordinate().Should().Be(31); // In Java/C# bitwise & works on 2s complement
    }

    [Fact]
    public void ChunkIndex_ShouldRoundTrip()
    {
        int x = 123;
        int z = -456;
        long index = ChunkExtensions.GetChunkIndex(x, z);
        
        ChunkExtensions.GetChunkX(index).Should().Be(x);
        ChunkExtensions.GetChunkZ(index).Should().Be(z);
    }

    [Fact]
    public void IsChunkBorder_ShouldIdentifyBorders()
    {
        ChunkExtensions.IsChunkBorder(0, 5).Should().BeTrue();
        ChunkExtensions.IsChunkBorder(31, 5).Should().BeTrue();
        ChunkExtensions.IsChunkBorder(5, 0).Should().BeTrue();
        ChunkExtensions.IsChunkBorder(5, 31).Should().BeTrue();
        
        ChunkExtensions.IsChunkBorder(5, 5).Should().BeFalse();
        ChunkExtensions.IsChunkBorder(32, 5).Should().BeTrue(); // 32 is local 0
    }
}
