using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<InteractionType>))]
public enum InteractionType : byte
{
    Primary = 0,
    Secondary = 1,
    Ability1 = 2,
    Ability2 = 3,
    Ability3 = 4,
    Use = 5,
    Pick = 6,
    Pickup = 7,
    CollisionEnter = 8,
    CollisionLeave = 9,
    Collision = 10,
    EntityStatEffect = 11,
    SwapTo = 12,
    SwapFrom = 13,
    Death = 14,
    Wielding = 15,
    ProjectileSpawn = 16,
    ProjectileHit = 17,
    ProjectileMiss = 18,
    ProjectileBounce = 19,
    Held = 20,
    HeldOffhand = 21,
    Equipped = 22,
    Dodge = 23,
    GameModeSwap = 24
}
