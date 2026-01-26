using Lithium.SourceGenerators.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed partial record ProtocolVersion(
    [property: ToStringInclude] string Hash
);