using System.Runtime.Versioning;

namespace Hytale.Server;

public static class OperatingSystemExtensions
{
    extension(OperatingSystem)
    {
        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
        public static bool IsSupported() =>
            OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS();
    }
}