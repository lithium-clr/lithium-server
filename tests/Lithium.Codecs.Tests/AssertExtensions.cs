using System.Buffers;

namespace Lithium.Codecs.Tests;

public static class AssertExtensions
{
    extension(Assert)
    {
        public static void EncodeDecodeEqual<TCodec, TValue>(TValue value)
            where TCodec : ICodec<TValue>, new()
        {
            // Arrange
            var codec = new TCodec();
            var bufferWriter = new ArrayBufferWriter<byte>();

            // Act
            codec.Encode(value, bufferWriter);

            var sequence = new ReadOnlySequence<byte>(bufferWriter.WrittenMemory);
            var reader = new SequenceReader<byte>(sequence);
            var decoded = codec.Decode(ref reader);

            // Assert
            Assert.Equal(value, decoded);
        }   
    }
}