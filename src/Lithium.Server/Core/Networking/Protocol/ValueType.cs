using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

/// <summary>
/// Defines the type of value used in a calculation, such as for stat modifiers.
/// </summary>
[JsonConverter(typeof(EnumStringConverter<ValueType>))]
public enum ValueType : byte
{
    /// <summary>
    /// The value is a percentage.
    /// </summary>
    Percent = 0,
    
    /// <summary>
    /// The value is an absolute number.
    /// </summary>
    Absolute = 1
}
