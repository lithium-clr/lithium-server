using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MovementType : byte
{
    None = 0,
    Idle = 1,
    Crouching = 2,
    Walking = 3,
    Running = 4,
    Sprinting = 5,
    Climbing = 6,
    Swimming = 7,
    Flying = 8,
    Sliding = 9,
    Rolling = 10,
    Mounting = 11,
    SprintMounting = 12
}
