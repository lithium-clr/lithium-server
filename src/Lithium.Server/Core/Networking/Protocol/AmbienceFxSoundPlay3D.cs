using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<AmbienceFxSoundPlay3D>))]
public enum AmbienceFxSoundPlay3D : byte
{
    Random = 0,
    LocationName = 1,
    No = 2
}