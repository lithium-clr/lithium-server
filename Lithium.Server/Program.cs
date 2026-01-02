using System.Reflection;
using Lithium.Server;
using Lithium.Server.Core;
using Lithium.Server.Core.Logging;
using Lithium.Server.Core.Networking;
using Lithium.Server.Core.Networking.Extensions;
using Lithium.Server.Core.Systems.Commands;
using Lithium.Server.Dashboard;
using Serilog;
using Serilog.Events;
using Log = Serilog.Log;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();

SentrySdk.Init(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"];
    options.Environment = builder.Configuration["Environment"];

    options.SetBeforeSendLog(static log =>
    {
        // Filter out all info logs
        return log.Level switch
        {
            SentryLogLevel.Error or SentryLogLevel.Fatal => log,
            _ => null
        };
    });
});

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Sentry(s =>
    {
        s.Dsn = builder.Configuration["Sentry:Dsn"];
        s.Environment = builder.Configuration["Environment"];
        s.MinimumBreadcrumbLevel = LogEventLevel.Debug;
        s.MinimumEventLevel = LogEventLevel.Warning;
        s.AttachStacktrace = true;
        s.SendDefaultPii = false;
        s.Debug = true;
        s.SendDefaultPii = true;
        s.DiagnosticLevel = SentryLevel.Error;
        s.AttachStacktrace = true;
        s.EnableLogs = true;
        s.TracesSampleRate = 1f;
        s.SetBeforeSendLog(static log =>
        {
            return log.Level switch
            {
                SentryLogLevel.Warning or SentryLogLevel.Error or SentryLogLevel.Fatal => log,
                _ => null
            };
        });
    })
    .CreateLogger();

builder.Services.AddSerilog();

builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
//

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
builder.Services.AddConsoleCommands();

var app = builder.Build();

app.MapHub<ServerHub>("/hub/admin");

await app.RunAsync();