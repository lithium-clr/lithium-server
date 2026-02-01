using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

/// <summary>
/// Defines client-side events that can trigger an item reticle change.
/// </summary>
[JsonConverter(typeof(EnumStringConverter<ItemReticleClientEvent>))]
public enum ItemReticleClientEvent : byte
{
    OnHit = 0,
    Wielding = 1,
    OnMovementLeft = 2,
    OnMovementRight = 3,
    OnMovementBack = 4
}
