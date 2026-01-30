namespace Lithium.Server.Core;

// The java version read the manifest file (META-INF/MANIFEST.MF) to get the following informations,
// but in our cases, we dont have a manifest file, so we need to hardcode it for now.
// This class is the equivalent of the ManifestUtil class at com.hypixel.hytale.common.util.java
public static class ManifestConstants
{
    // Implementation-Branch
    public const string Branch = "release";

    // Implementation-Build
    public const string Build = "NoJar";

    // Implementation-Patchline
    public const string PatchLine = "release";

    // Implementation-Revision-Id
    public const string RevisionId = "87d03be09c7f114f385971a3caa437033671b771";

    // Implementation-Title
    public const string Title = "Server";

    // Implementation-Vendor-Id
    public const string VendorId = "com.hypixel.hytale";

    // Implementation-Version
    public const string Version = "2026.01.28-87d03be09";
}