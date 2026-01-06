using Lithium.Snowflake;

namespace Lithium.Server.Dashboard.Models;

public class Server
{
    public SnowflakeId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ServerStatus Status { get; set; }
    public string Icon { get; set; } = "dns"; // Default icon
}

public enum ServerStatus
{
    Running,
    Stopped,
    Starting,
    Stopping,
    Error
}