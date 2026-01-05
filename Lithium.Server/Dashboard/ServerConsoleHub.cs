using Lithium.Server.Core.Systems.Commands;
using Microsoft.AspNetCore.SignalR;

namespace Lithium.Server.Dashboard;

public sealed class ServerConsoleHub(
    ConsoleCommandRegistry registry,
    ConsoleCommandExecutor executor
) : Hub<IServerConsoleHub>
{
    public async Task ExecuteCommand(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine)) return;

        if (!commandLine.StartsWith('/')) return;
        
        // Remove leading slash if present before sending, or handle on server
        commandLine = commandLine[1..];
        
        var parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length is 0) return;

        var commandName = parts[0];
        var args = parts.Length > 1 ? parts[1..] : [];

        if (registry.TryGet(commandName, out var command))
        {
            try
            {
                await executor.ExecuteAsync(command, args);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error executing command '{Command}'", commandName);
            }
        }
        else
        {
            Serilog.Log.Warning("Unknown command: {Command}", commandName);
        }
    }

    public async Task RequestCommandSuggestions(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            await Clients.Caller.ReceiveCommandSuggestions([]);
            return;
        }

        // Remove the leading slash if present for searching
        var search = input.StartsWith('/') ? input[1..] : input;

        var suggestions = registry.Commands.Keys
            .Where(k => k.StartsWith(search, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToList();

        await Clients.Caller.ReceiveCommandSuggestions(suggestions.ToArray());
    }
}