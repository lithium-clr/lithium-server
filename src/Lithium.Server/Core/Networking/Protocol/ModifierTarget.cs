using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

/// <summary>
/// Specifies which part of a range a modifier applies to.
/// </summary>
[JsonConverter(typeof(EnumStringConverter<ModifierTarget>))]
public enum ModifierTarget : byte
{
    Min = 0,
    Max = 1
}
