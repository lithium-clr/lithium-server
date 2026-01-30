using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<BlockSoundEvent>))]
public enum BlockSoundEvent : byte
{
    Walk = 0,
    Land = 1,
    MoveIn = 2,
    MoveOut = 3,
    Hit = 4,
    Break = 5,
    Build = 6,
    Clone = 7,
    Harvest = 8
}