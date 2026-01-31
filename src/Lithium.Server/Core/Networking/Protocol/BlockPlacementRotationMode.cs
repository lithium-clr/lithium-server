using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<BlockPlacementRotationMode>))]
public enum BlockPlacementRotationMode : byte
{
    FacingPlayer = 0,
    StairFacingPlayer = 1,
    BlockNormal = 2,
    Default = 3
}