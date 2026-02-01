using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

/// <summary>
/// Defines different curve types for animations or transitions.
/// </summary>
[JsonConverter(typeof(EnumStringConverter<CurveType>))]
public enum CurveType : byte
{
    /// <summary>
    /// A linear transition.
    /// </summary>
    Linear = 0,

    /// <summary>
    /// An ease-in transition using a quartic function.
    /// </summary>
    QuartIn = 1,

    /// <summary>
    /// An ease-out transition using a quartic function.
    /// </summary>
    QuartOut = 2,

    /// <summary>
    /// An ease-in and ease-out transition using a quartic function.
    /// </summary>
    QuartInOut = 3
}
