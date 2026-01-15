using System.Buffers;
using System.Collections.Generic;
using Lithium.Codecs.Collections;
using Lithium.Codecs.Primitives;
using Xunit;

namespace Lithium.Codecs.Tests.Collections;

public class ListCodecTests
{
    [Fact]
    public void ListOfInt_Should_EncodeAndDecode()
    {
        // Arrange
        var elementCodec = new IntegerCodec();
        var listCodec = new ListCodec<int>(elementCodec);
        var value = new List<int> { 1, 2, 3, 10, -50, int.MaxValue };
        var bufferWriter = new ArrayBufferWriter<byte>();

        // Act
        listCodec.Encode(value, bufferWriter);
        
        var sequence = new ReadOnlySequence<byte>(bufferWriter.WrittenMemory);
        var reader = new SequenceReader<byte>(sequence);
        var decoded = listCodec.Decode(ref reader);

        // Assert
        Assert.Equal(value, decoded);
    }

    [Fact]
    public void ListOfString_Should_EncodeAndDecode()
    {
        // Arrange
        var elementCodec = new StringCodec();
        var listCodec = new ListCodec<string?>(elementCodec);
        var value = new List<string?> { "Hello", "World", null, "", "Lithium" };
        var bufferWriter = new ArrayBufferWriter<byte>();
        
        // Act
        listCodec.Encode(value, bufferWriter);
        
        var sequence = new ReadOnlySequence<byte>(bufferWriter.WrittenMemory);
        var reader = new SequenceReader<byte>(sequence);
        var decoded = listCodec.Decode(ref reader);

        // Assert
        Assert.Equal(value, decoded);
    }

    [Fact]
    public void EmptyList_Should_EncodeAndDecode()
    {
        // Arrange
        var elementCodec = new IntegerCodec();
        var listCodec = new ListCodec<int>(elementCodec);
        var value = new List<int>();
        var bufferWriter = new ArrayBufferWriter<byte>();

        // Act
        listCodec.Encode(value, bufferWriter);
        
        var sequence = new ReadOnlySequence<byte>(bufferWriter.WrittenMemory);
        var reader = new SequenceReader<byte>(sequence);
        var decoded = listCodec.Decode(ref reader);

        // Assert
        Assert.NotNull(decoded);
        Assert.Empty(decoded);
    }

    [Fact]
    public void NullList_Should_EncodeAndDecode()
    {
        // Arrange
        var elementCodec = new IntegerCodec();
        var listCodec = new ListCodec<int>(elementCodec);
        var bufferWriter = new ArrayBufferWriter<byte>();
        List<int>? value = null;

        // Act
        listCodec.Encode(value, bufferWriter);
        
        var sequence = new ReadOnlySequence<byte>(bufferWriter.WrittenMemory);
        var reader = new SequenceReader<byte>(sequence);
        var decoded = listCodec.Decode(ref reader);

        // Assert
        Assert.Null(decoded);
    }
}
