using Lithium.Codecs.Primitives;

namespace Lithium.Codecs.Tests.Primitives;

public sealed class DoubleCodecTests
{
    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void DoubleCodec_Should_EncodeAndDecode(double value)
    {
        Assert.EncodeDecodeEqual<DoubleCodec, double>(value);
    }
}
