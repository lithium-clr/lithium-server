using System.Reflection;
using Lithium.Core.ECS;
using Lithium.Server;
using Lithium.Server.Core;
using Lithium.Server.Core.Logging;
using Lithium.Server.Core.Networking;
using Lithium.Server.Core.Networking.Extensions;
using Lithium.Server.Core.Systems.Commands;
using Lithium.Server.Core.Systems.Commands.Parsers;
using Lithium.Server.Dashboard;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();

builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "[HH:mm:ss] ";
    options.IncludeScopes = false;
});

builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);

// SignalR
builder.Services.AddSignalR();

// Core services
builder.Services.AddSingleton<IServerConfigurationProvider, JsonServerConfigurationProvider>();
builder.Services.AddSingleton<ILoggerService, LoggerService>();
builder.Services.AddSingleton<IClientManager, ClientManager>();
builder.Services.AddSingleton<IPluginRegistry, PluginRegistry>();
builder.Services.AddSingleton<IPluginManager, PluginManager>();

builder.Services.AddPacketHandlers(Assembly.GetExecutingAssembly());

// Networking
builder.Services.AddSingleton<QuicServer>();

// Lifetime
builder.Services.AddHostedService<ServerLifetime>();
builder.Services.AddHostedService<WorldService>();

// Console command service
builder.Services.AddSingleton(
    new ConsoleCommandRegistry([
        typeof(Program).Assembly
    ])
);

builder.Services.AddSingleton<CommandArgumentParserRegistry>();
builder.Services.AddSingleton<CommandArgumentBinder>();
builder.Services.AddSingleton<ConsoleCommandExecutor>();
builder.Services.AddSingleton<CoreCommands>();

builder.Services.AddSingleton<ICommandArgumentParser, StringParser>();
builder.Services.AddSingleton<ICommandArgumentParser, BoolParser>();
builder.Services.AddSingleton<ICommandArgumentParser, Vector3Parser>();

builder.Services.AddHostedService<CommandParserInitializer>();
builder.Services.AddHostedService<ConsoleInputService>();

var app = builder.Build();

app.MapHub<ServerHub>("/hub/admin");

await app.RunAsync();