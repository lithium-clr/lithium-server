using Lithium.Codecs;
using Lithium.SourceGenerators.Attributes;

namespace Lithium.Server.Common;

[Codec]
public sealed partial class AuthorInfo
{
    [ToStringInclude] public string? Name { get; set; }
    [ToStringInclude] public string? Email { get; set; }
    [ToStringInclude] public string? Url { get; set; }
}