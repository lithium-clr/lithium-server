using FluentAssertions;
using Lithium.Core.Extensions;

namespace Lithium.Core.Tests.Extensions;

public class BitExtensionsTests
{
    [Fact]
    public void Nibble_ShouldRoundTripCorrectly()
    {
        byte[] data = new byte[4];
        
        // Test Index 0 (Even) -> High Nibble (per Java impl)
        data.SetNibble(0, 0xA); // 1010
        data.GetNibble(0).Should().Be(0xA);
        data[0].Should().Be(0xA0); // High is A, Low is 0
        
        // Test Index 1 (Odd) -> Low Nibble (per Java impl)
        data.SetNibble(1, 0xB); // 1011
        data.GetNibble(1).Should().Be(0xB);
        data[0].Should().Be(0xAB); // High is A (preserved), Low is B
        
        // Verify Index 0 is still 0xA
        data.GetNibble(0).Should().Be(0xA);
    }

    [Fact]
    public void SetNibble_ShouldMaskValue()
    {
        byte[] data = new byte[1];
        data.SetNibble(0, 0xFF); // Should be truncated to 0xF
        data.GetNibble(0).Should().Be(0xF);
    }
}
