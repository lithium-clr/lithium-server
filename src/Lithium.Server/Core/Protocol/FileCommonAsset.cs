using Lithium.SourceGenerators.Attributes;

namespace Lithium.Server.Core.Protocol;

public sealed partial class FileCommonAsset(string filePath, string name, string hash) : CommonAsset(name, hash)
{
    [ToStringInclude] public string FilePath { get; } = filePath;
    
    protected override async Task<BlobData> ReadBlobAsync()
    {
        if (!File.Exists(FilePath))
            return BlobData.Empty;

        var data = await File.ReadAllBytesAsync(FilePath);
        return new BlobData(data);
    }
}