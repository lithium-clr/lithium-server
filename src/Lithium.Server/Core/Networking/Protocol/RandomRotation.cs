using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<RandomRotation>))]
public enum RandomRotation : byte
{
    None = 0,
    YawPitchRollStep1 = 1,
    YawStep1 = 2,
    YawStep1XZ = 3,
    YawStep90 = 4
}