using Lithium.Codecs.Primitives;

namespace Lithium.Codecs.Tests.Primitives;

public sealed class LongCodecTests
{
    [Theory]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(-1L)]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    public void LongCodec_Should_EncodeAndDecode(long value)
    {
        Assert.EncodeDecodeEqual<LongCodec, long>(value);
    }
}
