using System.Reflection;
using Lithium.Codecs;
using Lithium.Server;
using Lithium.Server.AssetStore;
using Lithium.Server.Core;
using Lithium.Server.Core.Codecs;
using Lithium.Server.Core.Logging;
using Lithium.Server.Core.Networking;
using Lithium.Server.Core.Networking.Authentication;
using Lithium.Server.Core.Networking.Authentication.OAuth;
using Lithium.Server.Core.Networking.Extensions;
using Lithium.Server.Core.Protocol;
using Lithium.Server.Core.Semver;
using Lithium.Server.Core.Storage;
using Lithium.Server.Core.Systems.Commands;
using Lithium.Server.Dashboard;
using Serilog;
using Serilog.Events;
using Log = Serilog.Log;

var builder = WebApplication.CreateBuilder(args);

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
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
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.With<ShortSourceContextEnricher>()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} | {SourceContext:l}] {Message:lj}{NewLine}{Exception}")
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
                SentryLogLevel.Error or SentryLogLevel.Fatal => log,
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

builder.Services.AddHttpClient();

// Hytale authentication services
builder.Services.Configure<SessionServiceOptions>(options => { options.Url = AuthConstants.SessionServiceUrl; });
builder.Services.AddSingleton<ISessionServiceProvider, SessionServiceProvider>();
builder.Services.AddSingleton<ISessionServiceClient, SessionServiceClient>();

// Register the credential store
// Use FileAuthCredentialStore by default for persistence
builder.Services.Configure<FileStoreOptions>(options => { options.Path = Path.Combine(AppContext.BaseDirectory); });
builder.Services.AddSingleton<IAuthCredentialStore, AuthCredentialStore>();

builder.Services.AddSingleton<IServerAuthManager, ServerAuthManager>();
builder.Services.AddSingleton<OAuthClient>();
builder.Services.AddSingleton<IOAuthDeviceFlow, AuthDeviceFlow>();
builder.Services.Configure<JwtValidatorOptions>(options =>
{
    options.Audience = "";
    options.Issuer = AuthConstants.SessionServiceUrl;
    options.JwksUri = "https://sessions.hytale.com/.well-known/jwks.json";
});
builder.Services.AddSingleton<JwtValidator>();

// Codecs
builder.Services.AddLithiumCodecs()
    .AddSingleton<ICodec<Lithium.Server.Core.Semver.Semver>, SemverCodec>()
    .AddSingleton<ICodec<SemverRange>, SemverRangeCodec>();

builder.Services.AddJwkKeyCodec();
builder.Services.AddJwksResponseCodec();
builder.Services.AddAccessTokenResponseCodec();
builder.Services.AddAuthCredentialsCodec();
builder.Services.AddAuthGrantResponseCodec();
builder.Services.AddGameProfileCodec();
builder.Services.AddGameSessionResponseCodec();

// Core services
builder.Services.AddSingleton<HytaleServer>();
builder.Services.AddSingleton<IServerConfigurationProvider, ServerConfigurationProvider>();
builder.Services.AddSingleton<ILoggerService, LoggerService>();
builder.Services.AddSingleton<IClientManager, ClientManager>();
builder.Services.AddSingleton<IPluginRegistry, PluginRegistry>();
builder.Services.AddSingleton<IPluginManager, PluginManager>();
builder.Services.AddSingleton<IServerManager, ServerManager>();
builder.Services.AddSingleton<PlayerCommonAssets>();
// builder.Services.AddSingleton<CommonAssetModule>();
builder.Services.AddSingleton<CommonAssetRegistry>();
// builder.Services.AddSingleton<AssetModule>();
builder.Services.AddSingleton<AssetManager>();
builder.Services.AddSingleton<AssetLoader>();

builder.Services.AddPacketHandlers(Assembly.GetExecutingAssembly());

// Networking
builder.Services.AddSingleton<QuicServer>();

// Lifetime
builder.Services.AddSingleton<ServerLifetime>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<ServerLifetime>());
builder.Services.AddHostedService<SignalRLogForwarder>();

// Console command service
builder.Services.AddConsoleCommands();

var app = builder.Build();

// Manually initialize routers to avoid circular dependency
RouterInitializer.InitializeRouters(app.Services);

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