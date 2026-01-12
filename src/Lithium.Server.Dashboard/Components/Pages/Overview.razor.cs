using Lithium.Server.Dashboard.Models;
using Lithium.Snowflake;
using Microsoft.AspNetCore.Components;

namespace Lithium.Server.Dashboard.Components.Pages;

public partial class Overview : ComponentBase
{
    [Parameter] public string ServerId { get; set; } = string.Empty;

    private Models.Server? _server;
    // private List<ActivityItem> _activityItems = [];
    private List<double> _cpuData = [];
    private List<double> _memoryData = [];

    protected override void OnInitialized()
    {
        // Simulate fetching server data based on ServerId
        if (long.TryParse(ServerId, out var serverId))
        {
            // In a real app, you would fetch this from a service
            _server = new Models.Server
            {
                Id = new SnowflakeId(serverId),
                Name = $"Server {serverId.ToString()}",
                Icon = "server",
                Status = ServerStatus.Running
            };
        }

        // _activityItems = new List<ActivityItem>
        // {
        //     new() { Icon = "update", Title = "System Update", Description = $"Server '{_server?.Name}' was updated.", Timestamp = DateTime.Now.AddMinutes(-5) },
        //     new() { Icon = "person_add", Title = "New User", Description = "User 'JohnDoe' registered.", Timestamp = DateTime.Now.AddHours(-1) },
        // };

        var random = new Random();
        _cpuData = Enumerable.Range(0, 100).Select(_ => random.NextDouble() * 100).ToList();
        _memoryData = Enumerable.Range(0, 100).Select(_ => random.NextDouble() * 100).ToList();
    }
}