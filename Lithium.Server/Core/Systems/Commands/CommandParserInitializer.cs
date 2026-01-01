namespace Lithium.Server.Core.Systems.Commands;

public sealed class CommandParserInitializer(
    CommandArgumentParserRegistry registry,
    IServiceProvider services
) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();

        var parsers = scope.ServiceProvider.GetServices<ICommandArgumentParser>();
        
        foreach (var parser in parsers)
            registry.Register(parser);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}