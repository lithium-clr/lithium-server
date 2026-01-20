// // See https://aka.ms/new-console-template for more information
//
// using System.Net;
// using System.Reflection;
// using Lithium.Client;
// using Lithium.Client.Core.Networking;
// using Lithium.Client.Core.Networking.Extensions;
// using Lithium.Core;
// using Lithium.Core.Extensions;
// using Lithium.Core.Networking;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
//
// var builder = Host.CreateApplicationBuilder(args);
//
// builder.Logging.ClearProviders();
//
// builder.Logging.AddSimpleConsole(options =>
// {
//     options.SingleLine = true;
//     options.TimestampFormat = "[HH:mm:ss] ";
//     options.IncludeScopes = false;
// });
//
// builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
// builder.Logging.AddFilter("System", LogLevel.Warning);
//
// builder.Services.AddSingleton(new QuicClientOptions
// {
//     EndPoint = new IPEndPoint(IPAddress.Loopback, 7777),
//     ApplicationProtocol = "hytale"
// });
//
// builder.Services.AddPacketHandlers(Assembly.GetExecutingAssembly());
//
// if (OperatingSystem.IsSupported())
// {
//     builder.Services.AddSingleton<QuicClient>();
// }
//
// builder.Services.AddHostedService<ClientLifetime>();
//
// var app = builder.Build();
//
// app.Run();