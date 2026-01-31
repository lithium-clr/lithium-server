using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<BlockParticleEvent>))]
public enum BlockParticleEvent : byte
{
    Walk = 0,
    Run = 1,
    Sprint = 2,
    SoftLand = 3,
    HardLand = 4,
    MoveOut = 5,
    Hit = 6,
    Break = 7,
    Build = 8,
    Physics = 9
}