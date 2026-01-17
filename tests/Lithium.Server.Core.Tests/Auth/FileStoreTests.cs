using System.Buffers;
using System.Text;
using FluentAssertions;
using Lithium.Codecs;
using Lithium.Server.Core.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Lithium.Server.Core.Tests.Auth;

public sealed class FileStoreTests
{
    private readonly TestCodec _codec;
    private readonly FileStore<TestData> _store;
    private readonly string _testStorePath = Path.Combine(Path.GetTempPath(), "test-store", nameof(TestData));

    public FileStoreTests()
    {
        var loggerMock = new Mock<ILogger<FileStore<TestData>>>();
        var optionsMock = new Mock<IOptions<FileSystemStoreOptions>>();
        
        optionsMock.Setup(o => o.Value).Returns(new FileSystemStoreOptions { Path = Path.Combine(Path.GetTempPath(), "test-store") });

        _codec = new TestCodec();
        _store = new TestFileStore(loggerMock.Object, optionsMock.Object, _codec);

        if (Directory.Exists(_testStorePath))
            Directory.Delete(_testStorePath, true);
        
        Directory.CreateDirectory(_testStorePath);
    }

    [Fact]
    public async Task SaveAsync_ShouldWriteFile()
    {
        // Arrange
        var id = "test-id";
        var data = new TestData { Value = "test-value" };
        var encodedData = Encoding.UTF8.GetBytes(data.Value);

        _codec.OnEncode = (_, writer) => writer.Write(encodedData);

        // Act
        await _store.SaveAsync(id, data);

        // Assert
        var filePath = Path.Combine(_testStorePath, $"{id}.bin");
        File.Exists(filePath).Should().BeTrue();
        
        var fileContent = await File.ReadAllBytesAsync(filePath);

        var expectedContent = new List<byte>();
        expectedContent.AddRange(StoreFileHeader.MagicBytes);
        expectedContent.Add(StoreFileHeader.CurrentVersion);
        expectedContent.AddRange(encodedData);

        fileContent.Should().BeEquivalentTo(expectedContent);
    }

    [Fact]
    public async Task LoadAsync_ShouldReturnData_WhenFileExists()
    {
        // Arrange
        var id = "test-id";
        var data = new TestData { Value = "test-value" };
        var encodedData = Encoding.UTF8.GetBytes(data.Value);
        var filePath = Path.Combine(_testStorePath, $"{id}.bin");

        var fileContent = new List<byte>();
        fileContent.AddRange(StoreFileHeader.MagicBytes);
        fileContent.Add(StoreFileHeader.CurrentVersion);
        fileContent.AddRange(encodedData);
        
        await File.WriteAllBytesAsync(filePath, fileContent.ToArray());

        _codec.DecodedValue = data;

        // Act
        var result = await _store.LoadAsync(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task LoadAsync_ShouldReturnDefault_WhenFileDoesNotExist()
    {
        // Arrange
        var id = "non-existent-id";

        // Act
        var result = await _store.LoadAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteFile()
    {
        // Arrange
        var id = "test-id";
        var filePath = Path.Combine(_testStorePath, $"{id}.bin");
        await File.WriteAllTextAsync(filePath, "test-content");

        // Act
        await _store.DeleteAsync(id);

        // Assert
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task ListAsync_ShouldReturnAllIds()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_testStorePath, "id1.bin"), "content");
        await File.WriteAllTextAsync(Path.Combine(_testStorePath, "id2.bin"), "content");
        await File.WriteAllTextAsync(Path.Combine(_testStorePath, "id3.txt"), "content");

        // Act
        var result = await _store.ListAsync();

        // Assert
        result.Should().BeEquivalentTo("id1", "id2");
    }



    [Fact]
    public async Task ListAsync_ShouldReturnFilteredIds()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_testStorePath, "id1.bin"), "content");
        await File.WriteAllTextAsync(Path.Combine(_testStorePath, "id2.bin"), "content");

        // Act
        var result = await _store.ListAsync("id1");
        
        // Assert
        result.Should().BeEquivalentTo("id1");
    }

    private sealed class TestData
    {
        public string? Value { get; init; }
    }

    private class TestCodec : ICodec<TestData>
    {
        public TestData? DecodedValue { get; set; }
        public Action<TestData, IBufferWriter<byte>>? OnEncode { get; set; }

        public TestData? Decode(ref SequenceReader<byte> reader)
        {
            // For the test, we don't need to actually read the sequence.
            // We just return the value we've been told to.
            return DecodedValue;
        }

        public void Encode(TestData value, IBufferWriter<byte> writer)
        {
            OnEncode?.Invoke(value, writer);
        }
    }

    private class TestFileStore(
        ILogger<FileStore<TestData>> logger,
        IOptions<FileSystemStoreOptions> options,
        ICodec<TestData> codec
    ) : FileStore<TestData>(logger, options, codec);
}