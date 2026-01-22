using Lithium.SourceGenerators.Attributes;

namespace Lithium.Server.Core.Protocol;

public sealed class AddCommonAssetResult
{
    [ToStringInclude] public PackAsset NewPackAsset { get; set; } = null!;
    [ToStringInclude] public PackAsset PreviousNameAsset { get; set; } = null!;
    [ToStringInclude] public PackAsset ActiveAsset { get; set; } = null!;
    [ToStringInclude] public PackAsset[] PreviousHashAssets { get; set; } = [];
    [ToStringInclude] public int DuplicateAssetId { get; set; }
}