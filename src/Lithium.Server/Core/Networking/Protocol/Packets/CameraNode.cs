using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[JsonConverter(typeof(EnumStringConverter<CameraNode>))]
public enum CameraNode
{
    None = 0,
    Head = 1,
    LShoulder = 2,
    RShoulder = 3,
    Belly = 4
}