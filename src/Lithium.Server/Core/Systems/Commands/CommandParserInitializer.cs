using System.Reflection;
using Microsoft.Extensions.Options;

namespace Lithium.Server.Core.Systems.Commands;

public sealed class ConsoleCommandRegisterOptions
{
    public IEnumerable<Assembly> Assemblies { get; set; } = [];
}

public sealed class ConsoleCommandRegister(
    CommandArgumentParserRegistry parserRegistry,
    ConsoleCommandRegistry commandRegistry,
    IOptions<ConsoleCommandRegisterOptions> options
) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var assembly in options.Value.Assemblies)
        {
            parserRegistry.RegisterAssembly(assembly);
            commandRegistry.RegisterAssembly(assembly);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}