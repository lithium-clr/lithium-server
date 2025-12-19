// See https://aka.ms/new-console-template for more information

using System.Net;
using Hytale.Client;
using Hytale.Client.Core.Networking;
using Hytale.Core.Networking;
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

builder.Services.AddSingleton(new QuicClientOptions
{
    EndPoint = new IPEndPoint(IPAddress.Loopback, 7777),
    ApplicationProtocol = "hytale"
});

builder.Services.AddSingleton<PacketRegistry>();

if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
{
    builder.Services.AddSingleton<IGameClient, QuicGameClient>();
}

builder.Services.AddHostedService<ClientLifetime>();

var app = builder.Build();

app.Run();