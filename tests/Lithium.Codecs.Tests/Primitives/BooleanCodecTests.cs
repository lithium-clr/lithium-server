using Lithium.Codecs.Primitives;

namespace Lithium.Codecs.Tests.Primitives;

public sealed class BooleanCodecTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BooleanCodec_Should_EncodeAndDecode(bool value)
    {
        Assert.EncodeDecodeEqual<BoolCodec, bool>(value);
    }
}