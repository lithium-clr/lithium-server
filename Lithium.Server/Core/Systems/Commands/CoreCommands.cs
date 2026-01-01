namespace Lithium.Server.Core.Systems.Commands;

public sealed class CoreCommands(
    IHostApplicationLifetime lifetime,
    ConsoleCommandRegistry registry
)
{
    [ConsoleCommand("ban", "Ban a player")]
    private void Ban(bool age)
    {
        Console.WriteLine("Ban: " + string.Join(", ", age));
    }
    
    [ConsoleCommand("stop", "Stop the server")]
    private void Stop()
    {
        lifetime.StopApplication();
    }

    [ConsoleCommand("help", "List available commands")]
    private void Help()
    {
        Console.WriteLine("Available commands:");
        
        foreach (var cmd in registry.Commands.Values)
            Console.WriteLine($"- {cmd.Name,-10} {cmd.Description}");
    }
}