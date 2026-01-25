using Lithium.SourceGenerators.Attributes;

namespace Lithium.Server.Core.Protocol;

public sealed record AddCommonAssetResult(
    [property: ToStringInclude] PackAsset NewPackAsset,
    [property: ToStringInclude] PackAsset? PreviousNameAsset,
    [property: ToStringInclude] PackAsset ActiveAsset,
    [property: ToStringInclude] int? DuplicateAssetId
);