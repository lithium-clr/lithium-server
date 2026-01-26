using Lithium.SourceGenerators.Attributes;

namespace Lithium.Server.Core;

public sealed partial record PackAsset(
    [property: ToStringInclude] string Pack,
    [property: ToStringInclude] CommonAsset Asset
);