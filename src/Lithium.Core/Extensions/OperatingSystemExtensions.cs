using System.Runtime.Versioning;

namespace Lithium.Core.Extensions;

public static class OperatingSystemExtensions
{
    extension(OperatingSystem)
    {
        [SupportedOSPlatformGuard("windows")]
        [SupportedOSPlatformGuard("linux")]
        [SupportedOSPlatformGuard("macos")]
        public static bool IsSupported() =>
            OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS();
    }
}