using Lithium.Codecs.Primitives;

namespace Lithium.Codecs.Tests.Primitives;

public sealed class ByteCodecTests
{
    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)1)]
    [InlineData(byte.MaxValue)]
    public void ByteCodec_Should_EncodeAndDecode(byte value)
    {
        Assert.EncodeDecodeEqual<ByteCodec, byte>(value);
    }
}
