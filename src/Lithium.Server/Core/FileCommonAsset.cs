using Lithium.Server.Core.Networking.Protocol;

namespace Lithium.Server.Core;

public sealed record FileCommonAsset(string FilePath, string Name, string Hash) : CommonAsset(Name, Hash)
{
    protected override async Task<BlobData> ReadBlobAsync()
    {
        if (!File.Exists(FilePath))
            return BlobData.Empty;

        var data = await File.ReadAllBytesAsync(FilePath);
        return new BlobData(data);
    }
}