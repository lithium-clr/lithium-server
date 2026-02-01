using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

/// <summary>
/// Defines sound events related to items.
/// </summary>
[JsonConverter(typeof(EnumStringConverter<ItemSoundEvent>))]
public enum ItemSoundEvent : byte
{
    /// <summary>
    /// The sound played when an item is dragged.
    /// </summary>
    Drag = 0,

    /// <summary>
    /// The sound played when an item is dropped.
    /// </summary>
    Drop = 1
}
