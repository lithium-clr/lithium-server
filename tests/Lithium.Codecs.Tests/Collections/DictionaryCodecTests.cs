using System.Buffers;
using System.Collections.Generic;
using Lithium.Codecs.Collections;
using Lithium.Codecs.Primitives;
using Xunit;

namespace Lithium.Codecs.Tests.Collections;

public sealed class DictionaryCodecTests
{
    [Fact]
    public void DictionaryOfStringToInt_Should_EncodeAndDecode()
    {
        // Arrange
        var keyCodec = new NonNullStringCodec();
        var valueCodec = new IntegerCodec();
        var dictionaryCodec = new DictionaryCodec<string, int>(keyCodec, valueCodec);
        
        var value = new Dictionary<string, int>
        {
            { "One", 1 },
            { "Two", 2 },
            { "Three", 3 }
        };
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        // Act
        dictionaryCodec.Encode(value, bufferWriter);
        
        var sequence = new ReadOnlySequence<byte>(bufferWriter.WrittenMemory);
        var reader = new SequenceReader<byte>(sequence);
        var decoded = dictionaryCodec.Decode(ref reader);

        // Assert
        Assert.Equal(value, decoded);
    }
    
    [Fact]
    public void DictionaryOfIntToString_Should_EncodeAndDecode()
    {
        // Arrange
        var keyCodec = new IntegerCodec();
        var valueCodec = new StringCodec();
        var dictionaryCodec = new DictionaryCodec<int, string?>(keyCodec, valueCodec);
        
        var value = new Dictionary<int, string?>
        {
            { 1, "One" },
            { 2, "Two" },
            { 3, null },
        };
        
        var bufferWriter = new ArrayBufferWriter<byte>();

        // Act
        dictionaryCodec.Encode(value, bufferWriter);
        
        var sequence = new ReadOnlySequence<byte>(bufferWriter.WrittenMemory);
        var reader = new SequenceReader<byte>(sequence);
        var decoded = dictionaryCodec.Decode(ref reader);

        // Assert
        Assert.Equal(value, decoded);
    }

    [Fact]
    public void EmptyDictionary_Should_EncodeAndDecode()
    {
        // Arrange
        var keyCodec = new NonNullStringCodec();
        var valueCodec = new IntegerCodec();
        var dictionaryCodec = new DictionaryCodec<string, int>(keyCodec, valueCodec);
        var value = new Dictionary<string, int>();
        var bufferWriter = new ArrayBufferWriter<byte>();

        // Act
        dictionaryCodec.Encode(value, bufferWriter);
        
        var sequence = new ReadOnlySequence<byte>(bufferWriter.WrittenMemory);
        var reader = new SequenceReader<byte>(sequence);
        var decoded = dictionaryCodec.Decode(ref reader);

        // Assert
        Assert.NotNull(decoded);
        Assert.Empty(decoded);
    }

    [Fact]
    public void NullDictionary_Should_EncodeAndDecode()
    {
        // Arrange
        var keyCodec = new NonNullStringCodec();
        var valueCodec = new IntegerCodec();
        var dictionaryCodec = new DictionaryCodec<string, int>(keyCodec, valueCodec);
        var bufferWriter = new ArrayBufferWriter<byte>();
        Dictionary<string, int>? value = null;
        
        // Act
        dictionaryCodec.Encode(value, bufferWriter);
        
        var sequence = new ReadOnlySequence<byte>(bufferWriter.WrittenMemory);
        var reader = new SequenceReader<byte>(sequence);
        var decoded = dictionaryCodec.Decode(ref reader);

        // Assert
        Assert.Null(decoded);
    }
}
