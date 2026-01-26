using Lithium.SourceGenerators.Attributes;

namespace Lithium.Server.Core;

public sealed record AddCommonAssetResult(
    PackAsset NewPackAsset,
    PackAsset? PreviousNameAsset,
    PackAsset ActiveAsset,
    int? DuplicateAssetId
);