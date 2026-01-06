namespace Lithium.Server.Core.Systems.Commands;

public sealed class CoreCommands(
    IHostApplicationLifetime lifetime,
    ConsoleCommandRegistry registry
)
{
    [ConsoleCommand("sentry", "Sentry test")]
    private void Sentry()
    {
        SentrySdk.CaptureMessage("Hello Sentry");
    }
    
    [ConsoleCommand("stop", "Stop the server")]
    private void Stop()
    {
        // throw new Exception("Stop");
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