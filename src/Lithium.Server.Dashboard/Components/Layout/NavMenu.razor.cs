using Lithium.Snowflake;
using Lithium.Snowflake.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Lithium.Server.Dashboard.Components.Layout;

public partial class NavMenu : IDisposable
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IIdGenerator IdGenerator { get; set; } = null!;

    private List<Models.Server> _servers = [];
    private SnowflakeId _activeServerId;
    private string _activeNavigation = "overview";

    private Models.Server? ActiveServer => _servers.FirstOrDefault(s => s.Id == _activeServerId);

    protected override void OnInitialized()
    {
        // Simulate fetching servers from a service
        _servers =
        [
            new Models.Server { Id = new SnowflakeId(1384530019614720), Name = "Main" },
            new Models.Server { Id = new SnowflakeId(1384530019614721), Name = "Dev" },
            new Models.Server { Id = new SnowflakeId(1384530019614722), Name = "Test" }
        ];

        NavigationManager.LocationChanged += OnLocationChanged;
        UpdateActiveServer(NavigationManager.Uri);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        UpdateActiveServer(e.Location);
        StateHasChanged();
    }

    private void UpdateActiveServer(string url)
    {
        var uri = new Uri(url);
        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (_servers.Count is 0) return;

        switch (segments)
        {
            case ["servers", var serverIdString, var navigationId]:
                _activeServerId = new SnowflakeId(long.Parse(serverIdString));
                _activeNavigation = navigationId;
                break;
            case ["servers", var serverIdString2, ..]:
                _activeServerId = new SnowflakeId(long.Parse(serverIdString2));
                _activeNavigation = "overview";
                break;
        }
    }

    private void OnNavigationChanged()
    {
        UpdateActiveServer(NavigationManager.Uri);
    }

    void IDisposable.Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}