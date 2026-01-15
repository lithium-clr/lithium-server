using Lithium.Codecs.Primitives;

namespace Lithium.Codecs.Tests.Primitives;

public sealed class StringCodecTests
{
    [Theory]
    [InlineData("Hello, World!")]
    [InlineData("Hytale")]
    [InlineData("™")]
    [InlineData("Lithium")]
    [InlineData("")]
    [InlineData(null)]
    public void StringCodec_Should_EncodeAndDecode(string? value)
    {
        Assert.EncodeDecodeEqual<StringCodec, string?>(value);
    }
}
