using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

/// <summary>
/// Defines how a modifier's value is applied.
/// </summary>
[JsonConverter(typeof(EnumStringConverter<CalculationType>))]
public enum CalculationType : byte
{
    Additive = 0,
    Multiplicative = 1
}
