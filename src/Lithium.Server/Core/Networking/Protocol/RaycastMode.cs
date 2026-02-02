using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<RaycastMode>))]
public enum RaycastMode : byte
{
    FollowMotion = 0,
    FollowLook = 1
}