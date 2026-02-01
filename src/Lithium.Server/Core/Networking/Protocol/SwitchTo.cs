using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

/// <summary>
/// Defines how a model switches between its normal and alternative appearance.
/// </summary>
[JsonConverter(typeof(EnumStringConverter<SwitchTo>))]
public enum SwitchTo : byte
{
    /// <summary>
    /// The model disappears.
    /// </summary>
    Disappear = 0,

    /// <summary>
    /// The model switches to a post-color effect.
    /// </summary>
    PostColor = 1,

    /// <summary>
    /// The model switches with a distortion effect.
    /// </summary>
    Distortion = 2,

    /// <summary>
    /// The model becomes transparent.
    /// </summary>
    Transparency = 3
}
