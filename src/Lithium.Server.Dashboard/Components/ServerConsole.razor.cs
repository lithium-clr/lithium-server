using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;

namespace Lithium.Server.Dashboard.Components;

public partial class ServerConsole : ComponentBase, IAsyncDisposable
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    private HubConnection? _hubConnection;
    private readonly List<(DateTimeOffset, int, string)> _logs = [];
    private string _connectionStatus = "Connecting...";
    private string _commandInput = "";

    // Suggestions
    private List<string> _suggestions = [];
    private int _selectedSuggestionIndex = -1;
    private bool _showSuggestions;
    private bool _preventKeyDefault;

    // Filtering
    private LogLevel? _activeFilter;
    
    private IEnumerable<(DateTimeOffset, int, string)> FilteredLogs => 
        _activeFilter.HasValue 
            ? _logs.Where(l => l.Item2 == (int)_activeFilter.Value) 
            : _logs;

    protected override async Task OnInitializedAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7144/hub/console")
            .WithAutomaticReconnect()
            .Build();

        _logs.Add((DateTime.Now, 2, "This is an information log."));
        _logs.Add((DateTime.Now, 3, "This is a warning."));
        _logs.Add((DateTime.Now, 4, "This is an error!"));
        
        _hubConnection.On<DateTimeOffset, int, string>("ReceiveLog", async (timestamp, level, message) =>
        {
            await InvokeAsync( () =>
            {
                _logs.Add((timestamp, level, message));
                StateHasChanged();
            });
        });

        _hubConnection.On<List<string>>("ReceiveCommandSuggestions", (suggestions) =>
        {
            InvokeAsync(() =>
            {
                _suggestions = suggestions;
                _selectedSuggestionIndex = -1;
                _showSuggestions = _suggestions.Count > 0;
                StateHasChanged();
            });
        });

        _hubConnection.Reconnecting += error =>
        {
            InvokeAsync(() =>
            {
                _connectionStatus = "Connection lost. Reconnecting...";
                StateHasChanged();
            });
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            InvokeAsync(() =>
            {
                _connectionStatus = "Connected";
                StateHasChanged();
            });
            return Task.CompletedTask;
        };

        try
        {
            await _hubConnection.StartAsync();
            _connectionStatus = "Connected";
        }
        catch (Exception ex)
        {
            _connectionStatus = $"Connection failed: {ex.Message}";
        }
    }

    private void ClearLogs()
    {
        _logs.Clear();
    }

    private void SetFilter(LogLevel? level)
    {
        if (_activeFilter == level)
        {
            _activeFilter = null; // Toggle off if same filter is clicked
        }
        else
        {
            _activeFilter = level;
        }
    }

    private string GetFilterButtonClass(LogLevel level)
    {
        return _activeFilter != level ? "opacity-50 hover:opacity-100" : "opacity-100";
    }

    private async Task HandleInput(ChangeEventArgs e)
    {
        _commandInput = e.Value?.ToString() ?? "";
        
        if (_commandInput.StartsWith('/'))
        {
            if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.SendAsync("RequestCommandSuggestions", _commandInput);
            }
        }
        else
        {
            _showSuggestions = false;
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        _preventKeyDefault = false;

        if (_showSuggestions && _suggestions.Count > 0)
        {
            if (e.Key == "ArrowUp")
            {
                _preventKeyDefault = true;
                _selectedSuggestionIndex--;
                if (_selectedSuggestionIndex < 0) _selectedSuggestionIndex = _suggestions.Count - 1;
                return;
            }
            else if (e.Key == "ArrowDown")
            {
                _preventKeyDefault = true;
                _selectedSuggestionIndex++;
                if (_selectedSuggestionIndex >= _suggestions.Count) _selectedSuggestionIndex = 0;
                return;
            }
            else if (e.Key == "Enter" || e.Key == "Tab")
            {
                if (_selectedSuggestionIndex >= 0 && _selectedSuggestionIndex < _suggestions.Count)
                {
                    _preventKeyDefault = true;
                    SelectSuggestion(_suggestions[_selectedSuggestionIndex]);
                    return;
                }
            }
            else if (e.Key == "Escape")
            {
                _showSuggestions = false;
                return;
            }
        }

        if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(_commandInput))
        {
            await SendCommand();
        }
    }

    private void SelectSuggestion(string suggestion)
    {
        _commandInput = "/" + suggestion + " ";
        _showSuggestions = false;
        _selectedSuggestionIndex = -1;
    }

    private async Task SendCommand()
    {
        if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected)
        {
            try
            {
                await _hubConnection.SendAsync("ExecuteCommand", _commandInput);
            }
            catch (Exception ex)
            {
                _logs.Add((DateTimeOffset.Now, (int)LogLevel.Error, $"Failed to send command: {ex.Message}"));
            }

            _commandInput = "";
            _showSuggestions = false;
        }
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_hubConnection is not null)
            await _hubConnection.DisposeAsync();
    }
}