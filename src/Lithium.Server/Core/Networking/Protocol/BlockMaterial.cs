using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<BlockMaterial>))]
public enum BlockMaterial : byte
{
    Empty = 0,
    Solid = 1
}