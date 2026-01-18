using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Auth;

public sealed class LauncherDataResponse
{
    [JsonPropertyName("profiles")] public GameProfile[] Profiles { get; init; } = [];
}