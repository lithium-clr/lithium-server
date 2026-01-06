namespace Lithium.Server.Core.Systems.Commands;

public sealed class ConsoleInputService(
    ILogger<ConsoleInputService> logger,
    ConsoleCommandRegistry registry,
    ConsoleCommandExecutor executor
) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _ = Task.Run(ReadLoop, CancellationToken.None);
        return Task.CompletedTask;
    }

    private async Task ReadLoop()
    {
        while (true)
        {
            var line = Console.ReadLine();
            if (line is null) return;

            var parts = CommandLineTokenizer.Tokenize(line);
            if (parts.Length is 0) continue;

            var name = parts[0].ToLowerInvariant();
            var args = parts.Skip(1).ToArray();

            if (!registry.TryGet(name, out var command))
            {
                logger.LogWarning("Unknown command: {Command}", name);
                continue;
            }

            try
            {
                await executor.ExecuteAsync(command, args);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Command execution failed: {Command}", name);
            }
        }
    }
}