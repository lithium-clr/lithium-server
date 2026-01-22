using Lithium.SourceGenerators.Attributes;

namespace Lithium.Server.Core.Protocol;

public sealed partial record ProtocolVersion(
    [property: ToStringInclude] string Hash
);