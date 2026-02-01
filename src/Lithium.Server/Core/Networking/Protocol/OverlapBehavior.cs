using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

/// <summary>
/// Defines behavior when elements overlap.
/// </summary>
[JsonConverter(typeof(EnumStringConverter<OverlapBehavior>))]
public enum OverlapBehavior : byte
{
    /// <summary>
    /// The new element extends the existing one.
    /// </summary>
    Extend = 0,

    /// <summary>
    /// The new element overwrites the existing one.
    /// </summary>
    Overwrite = 1,

    /// <summary>
    /// The new element is ignored.
    /// </summary>
    Ignore = 2
}
