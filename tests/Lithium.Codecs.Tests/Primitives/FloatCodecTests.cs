using Lithium.Codecs.Primitives;

namespace Lithium.Codecs.Tests.Primitives;

public sealed class FloatCodecTests
{
    [Theory]
    [InlineData(0.0f)]
    [InlineData(1.0f)]
    [InlineData(-1.0f)]
    [InlineData(float.MaxValue)]
    [InlineData(float.MinValue)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void FloatCodec_Should_EncodeAndDecode(float value)
    {
        Assert.EncodeDecodeEqual<FloatCodec, float>(value);
    }
}
