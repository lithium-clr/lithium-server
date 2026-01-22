using Lithium.SourceGenerators.Attributes;

namespace Lithium.Server.Core.Protocol;

public sealed class FileCommonAsset : CommonAsset
{
    [ToStringInclude] public string File { get; }

    public FileCommonAsset(string file, string name, byte[]? bytes) : base(name, bytes)
    {
        File = file;
    }

    public FileCommonAsset(string file, string name, string hash, byte[]? bytes) : base(name, hash, bytes)
    {
        File = file;
    }

    protected override Task<byte[]> GetBlob0()
    {
        return System.IO.File.ReadAllBytesAsync(File);
    }
}