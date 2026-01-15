using Lithium.Codecs.Primitives;

namespace Lithium.Codecs.Tests.Primitives;

public sealed class ShortCodecTests
{
    [Theory]
    [InlineData((short)0)]
    [InlineData((short)1)]
    [InlineData((short)-1)]
    [InlineData(short.MaxValue)]
    [InlineData(short.MinValue)]
    public void ShortCodec_Should_EncodeAndDecode(short value)
    {
        Assert.EncodeDecodeEqual<ShortCodec, short>(value);
    }
}
