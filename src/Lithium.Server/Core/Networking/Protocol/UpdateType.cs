using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<UpdateType>))]
public enum UpdateType : byte
{
    Init = 0,
    AddOrUpdate = 1,
    Remove = 2
}