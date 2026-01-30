using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<UpdateType>))]
public enum UpdateType : byte
{
    Init = 0,
    Update = 1
}