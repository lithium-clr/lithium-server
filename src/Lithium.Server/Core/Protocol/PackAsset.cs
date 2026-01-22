using Lithium.SourceGenerators.Attributes;

namespace Lithium.Server.Core.Protocol;

public sealed partial record PackAsset(
    [property: ToStringInclude] string Pack,
    [property: ToStringInclude] CommonAsset Asset
);