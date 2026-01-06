namespace Lithium.Server.Core.Systems.Commands;

public static class ConsoleInputExtensions
{
    public static IServiceCollection AddConsoleCommands(this IServiceCollection services)
    {
        services.Configure<ConsoleCommandRegisterOptions>(options =>
        {
            options.Assemblies =
            [
                typeof(Program).Assembly
            ];
        });

        services.AddSingleton<CommandArgumentParserRegistry>();
        services.AddSingleton<ConsoleCommandRegistry>();
        services.AddSingleton<CommandArgumentBinder>();
        services.AddSingleton<ConsoleCommandExecutor>();

        services.AddSingleton<CoreCommands>();

        services.AddHostedService<ConsoleCommandRegister>();
        services.AddHostedService<ConsoleInputService>();

        return services;
    }
}