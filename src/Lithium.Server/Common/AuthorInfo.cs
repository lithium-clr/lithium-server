using Lithium.Codecs;

namespace Lithium.Server.Common;

[Codec]
public sealed partial record AuthorInfo(
    string? Name,
    string? Email,
    string? Url
);