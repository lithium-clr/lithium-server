using System.Reflection;
using Lithium.Server;
using Lithium.Server.Core;
using Lithium.Server.Core.Logging;
using Lithium.Server.Core.Networking;
using Lithium.Server.Core.Networking.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();

builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "[HH:mm:ss] ";
    options.IncludeScopes = false;
});

builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);

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

var app = builder.Build();

app.Run();