using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

/// <summary>
/// Defines looping options for animations or effects.
/// </summary>
[JsonConverter(typeof(EnumStringConverter<LoopOption>))]
public enum LoopOption : byte
{
    /// <summary>
    /// The animation or effect plays once and then stops.
    /// </summary>
    PlayOnce = 0,

    /// <summary>
    /// The animation or effect loops indefinitely.
    /// </summary>
    Loop = 1,

    /// <summary>
    /// The animation or effect loops, playing forward and then in reverse.
    /// </summary>
    LoopMirror = 2
}
