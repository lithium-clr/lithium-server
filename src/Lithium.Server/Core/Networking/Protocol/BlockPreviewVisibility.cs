using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<BlockPreviewVisibility>))]
public enum BlockPreviewVisibility : byte
{
    AlwaysVisible = 0,
    AlwaysHidden = 1,
    Default = 2
}