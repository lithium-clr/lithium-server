using Lithium.Server.Core.Logging;

namespace Lithium.Server.Core;

public static class GlobalNamespace
{
    public static Logger Log { get; } = new("System");
}