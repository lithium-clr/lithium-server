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
using Spectre.Console;
using Log = Serilog.Log;

var builder = WebApplication.CreateBuilder(args);

AnsiConsole.Write(new FigletText(FigletFont.Default, "LITHIUM").LeftJustified().Color(Color.White));

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("https://localhost:7244") // Correct port for the web app
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

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
    .WriteTo.Sink(new SignalRSink())
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
builder.Services.AddHostedService<SignalRLogForwarder>();

// Console command service
builder.Services.AddConsoleCommands();

var app = builder.Build();

// Use CORS
app.UseCors();

app.MapHub<ServerHub>("/hub/admin");
app.MapHub<ServerConsoleHub>("/hub/console");

// Add a log to test the system
app.Lifetime.ApplicationStarted.Register(() =>
{
    Log.Information("Server has started. Logging to SignalR is active.");
});

await app.RunAsync();