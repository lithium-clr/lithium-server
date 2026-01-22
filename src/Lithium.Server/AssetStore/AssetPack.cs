using Lithium.Server.Common;
using Lithium.SourceGenerators.Attributes;

namespace Lithium.Server.AssetStore;

public sealed partial class AssetPack(string packLocation, string name, string root, bool isImmutable, PluginManifest manifest)
{
    [ToStringInclude] public string Name { get; } = name;
    [ToStringInclude] public string Root { get; } = root;
    
    public bool IsImmutable { get; private set; } = isImmutable;
    public PluginManifest Manifest { get; private set; } = manifest;
    public string PackLocation { get; private set; } = packLocation;

    public override bool Equals(object? obj)
    {
        if (this == obj) return true;
        return obj is AssetPack a && a.Name == Name && a.Root == Root;
    }

    public override int GetHashCode()
    {
        var result = Name.GetHashCode();
        return 31 * result + Root.GetHashCode();
    }
}