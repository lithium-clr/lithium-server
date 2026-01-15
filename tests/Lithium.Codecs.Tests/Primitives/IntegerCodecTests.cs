using Lithium.Codecs.Primitives;

namespace Lithium.Codecs.Tests.Primitives;

public sealed class IntegerCodecTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void IntegerCodec_Should_EncodeAndDecode(int value)
    {
        Assert.EncodeDecodeEqual<IntegerCodec, int>(value);
    }
}