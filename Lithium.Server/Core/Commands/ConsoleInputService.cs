namespace Lithium.Server.Core.Commands;

public sealed class ConsoleInputService(
    ILogger<ConsoleInputService> logger,
    ConsoleCommandRegistry registry,
    ConsoleCommandExecutor executor
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var line = Console.ReadLine();
            if (line is null) break;

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