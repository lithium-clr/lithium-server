using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<SoundCategory>))]
public enum SoundCategory : byte
{
    Music = 0,
    Ambient = 1,
    Sfx = 2,
    Ui = 3
}