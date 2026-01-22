using Lithium.Codecs;
using Lithium.Server.Core.Protocol;
using Lithium.Server.Core.Semver;

namespace Lithium.Server.Common;

[Codec]
public sealed partial class PluginManifest
{
    public string Group { get; set; } = null!;
    public string Name { get; set; } = null!;
    public Core.Semver.Semver Version { get; set; } = null!;
    public string? Description { get; set; }
    public List<AuthorInfo> Authors { get; set; } = [];
    public string? Website { get; set; }
    public string? Main { get; set; }
    public SemverRange? ServerVersion { get; set; }
    public Dictionary<PluginIdentifier, SemverRange> Dependencies { get; set; } = [];
    public Dictionary<PluginIdentifier, SemverRange> OptionalDependencies { get; set; } = [];
    public Dictionary<PluginIdentifier, SemverRange> LoadBefore { get; set; } = [];
    public bool DisabledByDefault { get; set; }
}