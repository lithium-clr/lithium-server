using Lithium.Server.Common;

namespace Lithium.Server.AssetStore;

/// <summary>
/// Represents a collection of assets and their associated metadata.
/// </summary>
/// <param name="Name">The name of the asset pack.</param>
/// <param name="Root">The root directory path where assets are located.</param>
/// <param name="Manifest">The plugin manifest associated with this pack.</param>
public sealed partial record AssetPack(
    string Name,
    string Root,
    PluginManifest Manifest
);