using System.Reflection;
using Hytale.Core.Networking;
using Hytale.Server;
using Hytale.Server.Core;
using Hytale.Server.Core.Networking;
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
builder.Services.AddSingleton<EventSystem>();
builder.Services.AddSingleton<ILoggerService, LoggerService>();
builder.Services.AddSingleton<IPluginRegistry, PluginRegistry>();
builder.Services.AddSingleton<IPluginManager, PluginManager>();
builder.Services.AddSingleton<IWorldService, WorldService>();
builder.Services.AddSingleton<IEntityManager, EntityManager>();

builder.Services.AddPacketHandlers(Assembly.GetExecutingAssembly());

// Networking
if (OperatingSystem.IsSupported())
{
    builder.Services.AddSingleton<IQuicServer, QuicServer>();
    // builder.Services.AddTransient<PacketHandler>();
}

// Lifetime
builder.Services.AddHostedService<ServerLifetime>();

var app = builder.Build();

app.Run();