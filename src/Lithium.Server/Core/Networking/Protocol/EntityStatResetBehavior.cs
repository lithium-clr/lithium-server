using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

/// <summary>
/// Defines how an entity's stat should behave when it resets.
/// </summary>
[JsonConverter(typeof(EnumStringConverter<EntityStatResetBehavior>))]
public enum EntityStatResetBehavior : byte
{
    /// <summary>
    /// The stat resets to its initial value.
    /// </summary>
    InitialValue = 0,

    /// <summary>
    /// The stat resets to its maximum value.
    /// </summary>
    MaxValue = 1
}
