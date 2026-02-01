using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<GameMode>))]
public enum GameMode : byte
{
    Adventure = 0,
    Creative = 1
}