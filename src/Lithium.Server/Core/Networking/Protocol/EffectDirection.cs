using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

/// <summary>
/// Defines the direction of a visual effect.
/// </summary>
[JsonConverter(typeof(EnumStringConverter<EffectDirection>))]
public enum EffectDirection : byte
{
    /// <summary>
    /// No specific direction.
    /// </summary>
    None = 0,

    /// <summary>
    /// The effect moves from bottom to top.
    /// </summary>
    BottomUp = 1,

    /// <summary>
    /// The effect moves from top to bottom.
    /// </summary>
    TopDown = 2,

    /// <summary>
    /// The effect moves towards the center.
    /// </summary>
    ToCenter = 3,

    /// <summary>
    /// The effect moves away from the center.
    /// </summary>
    FromCenter = 4
}
