using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol;

namespace Lithium.Server.Core.Resources;

public abstract record AssetResource : PacketObject
{
    // public string Id { get; init; } = string.Empty;
    
    public string? Parent { get; set; }
    
    [JsonIgnore]
    public string FileName { get; set; } = null!;
    
    public abstract PacketObject ToPacket();
}