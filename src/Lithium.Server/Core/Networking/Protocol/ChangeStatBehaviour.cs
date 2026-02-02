using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<ChangeStatBehaviour>))]
public enum ChangeStatBehaviour : byte
{
    Add = 0,
    Set = 1
}