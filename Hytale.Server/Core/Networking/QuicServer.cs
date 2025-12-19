using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography.X509Certificates;
using Hytale.Core.Networking;
using Microsoft.Extensions.Logging;

namespace Hytale.Server.Core.Networking;

public interface IQuicServer
{
    Task StartAsync(CancellationToken ct);
}

[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public sealed class QuicServer(
    ILogger<QuicServer> logger,
    PacketHandler packetHandler
) : IQuicServer
{
    private const string Protocol = "hytale";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var cert = X509CertificateLoader.LoadPkcs12FromFile("localhost.pfx", "devtest");

        logger.LogInformation("QUIC server starting");

        List<SslApplicationProtocol> protocols = [new(Protocol)];

        var listener = await QuicListener.ListenAsync(new QuicListenerOptions
        {
            ListenEndPoint = new IPEndPoint(IPAddress.Loopback, 7777),
            ApplicationProtocols = protocols,
            ConnectionOptionsCallback = (_, _, _) => ValueTask.FromResult(new QuicServerConnectionOptions
            {
                DefaultCloseErrorCode = 0,
                DefaultStreamErrorCode = 0,
                ServerAuthenticationOptions = new SslServerAuthenticationOptions
                {
                    ApplicationProtocols = protocols,
                    ServerCertificate = cert
                }
            })
        }, cancellationToken);

        logger.LogInformation("QUIC server listening");

        while (!cancellationToken.IsCancellationRequested)
        {
            var connection = await listener.AcceptConnectionAsync(cancellationToken);
            _ = Task.Run(() => packetHandler.HandleAsync(connection), cancellationToken);
        }
    }
}