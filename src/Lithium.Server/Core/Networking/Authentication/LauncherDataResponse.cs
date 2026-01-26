using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Authentication;

public sealed class LauncherDataResponse
{
    [JsonPropertyName("profiles")] public GameProfile[] Profiles { get; init; } = [];
}