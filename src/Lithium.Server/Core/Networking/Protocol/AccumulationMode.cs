using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<AccumulationMode>))]
public enum AccumulationMode : byte
{
    Set = 0,
    Sum = 1,
    Average = 2
}