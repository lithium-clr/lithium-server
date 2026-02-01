using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

/// <summary>
/// Defines the type of UI component attached to an entity.
/// </summary>
[JsonConverter(typeof(EnumStringConverter<EntityUIType>))]
public enum EntityUIType : byte
{
    /// <summary>
    /// A stat bar (e.g., health).
    /// </summary>
    EntityStat = 0,

    /// <summary>
    /// Floating combat text (e.g., damage numbers).
    /// </summary>
    CombatText = 1
}
