using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<ChangeVelocityType>))]
public enum ChangeVelocityType : byte
{
    Add = 0,
    Set = 1
}