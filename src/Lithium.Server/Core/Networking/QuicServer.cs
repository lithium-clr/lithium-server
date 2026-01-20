using System.Collections.Concurrent;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using Lithium.Server.Core.Auth;
using Lithium.Server.Core.Protocol;
using Lithium.Server.Core.Protocol.Packets.Connection;
using Lithium.Server.Core.Protocol.Transport;
using Lithium.Server.Dashboard;
using Microsoft.AspNetCore.SignalR;
using Org.BouncyCastle.Security;
using IPacket = Lithium.Server.Core.Protocol.IPacket;

namespace Lithium.Server.Core.Networking;

public sealed class QuicServer(
    ILogger<QuicServer> logger,
    IHubContext<ServerHub, IServerHub> hub,
    IHubContext<ServerConsoleHub, IServerConsoleHub> hubConsole,
    IPacketHandler packetHandler,
    IServerAuthManager serverAuthManager,
    ISessionServiceClient sessionServiceClient
) : IAsyncDisposable
{
    private const int DefaultPort = 5520;

    // private const int HeartbeatInterval = 15;
    private const string Protocol = "hytale/1";
    private const string CertificateFileName = "lithium_server_cert_v2.pfx";
    private const string CertificatePassword = "password";

    private QuicListener _listener = null!;
    private readonly Dictionary<QuicConnection, Channel> _channels = new();

    public async Task StartAsync(CancellationToken ct)
    {
        if (!QuicListener.IsSupported)
        {
            logger.LogInformation("QUIC is not supported on this platform.");
            return;
        }

        logger.LogInformation("Hytale QUIC Listener starting...");

        var cert = CertificateUtility.GetOrCreateSelfSignedCertificate(CertificateFileName, CertificatePassword);

        // serverAuthManager.SetServerCertificate(cert);

        // Use IPv6Any with DualMode (supported by default on Windows/Linux) to handle both IPv4 and IPv6
        var endpoint = new IPEndPoint(IPAddress.IPv6Any, DefaultPort);

        var serverAuthenticationOptions = new SslServerAuthenticationOptions
        {
            ApplicationProtocols = [new SslApplicationProtocol(Protocol)],
            ServerCertificate = cert,
            ClientCertificateRequired = false,
            RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
        };

        var quicServerConnectionOptions = new QuicServerConnectionOptions
        {
            ServerAuthenticationOptions = serverAuthenticationOptions,
            IdleTimeout = TimeSpan.FromSeconds(30),
            DefaultStreamErrorCode = 0x100,
            DefaultCloseErrorCode = 0x100,
        };

        var listenerOptions = new QuicListenerOptions
        {
            ListenEndPoint = endpoint,
            ApplicationProtocols = [new SslApplicationProtocol(Protocol)],
            ConnectionOptionsCallback = (connection, sslInfo, cancellationToken) =>
            {
                return ValueTask.FromResult(quicServerConnectionOptions);
            }
        };

        _listener = await QuicListener.ListenAsync(listenerOptions, ct);
        
        logger.LogInformation("Listening on {ListenerLocalEndPoint} with ALPN '{Protocol}'", _listener.LocalEndPoint,
            Protocol);

        // Start accepting connections
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var connection = await _listener.AcceptConnectionAsync(ct);
                logger.LogInformation("[{}] Connection accepted.", connection.RemoteEndPoint);
                _ = HandleConnectionAsync(connection);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError("Error accepting connection: {ExMessage}", ex.Message);
            }
        }
    }

    private async Task HandleConnectionAsync(QuicConnection connection)
    {
        try
        {
            while (true)
            {
                var stream = await connection.AcceptInboundStreamAsync();
                
                var channel = new Channel(connection, stream);
                _channels[connection] = channel;

                logger.LogInformation("[{ConnectionRemoteEndPoint}] Stream accepted: ID {StreamId}",
                    connection.RemoteEndPoint, stream.Id);

                await packetHandler.HandleAsync(channel);
                // _ = HandleStreamAsync(stream, connection.RemoteEndPoint);
            }
        }
        catch (QuicException ex) when (ex.QuicError == QuicError.ConnectionAborted)
        {
            logger.LogInformation("[{ConnectionRemoteEndPoint}] Connection aborted.", connection.RemoteEndPoint);
        }
        catch (Exception ex)
        {
            logger.LogError("[{ConnectionRemoteEndPoint}] Error: {ExMessage}", connection.RemoteEndPoint, ex.Message);
        }
        finally
        {
            await connection.DisposeAsync();
            logger.LogInformation("[{ConnectionRemoteEndPoint}] Connection closed.", connection.RemoteEndPoint);
        }
    }
    
    private async Task HandleStreamAsync(QuicStream stream, EndPoint remoteEndPoint)
    {
        try
        {
            // Loop to keep reading packets from the same stream
            while (true)
            {
                var header = new byte[8];
    
                if (!await ReadExactAsync(stream, header))
                {
                    // Stream closed by peer (EOF)
                    logger.LogInformation("[{RemoteEndPoint}] Stream ID {StreamId} closed by peer.", remoteEndPoint,
                        stream.Id);
                    break;
                }
    
                var payloadLength = BitConverter.ToInt32(header, 0);
                var packetId = BitConverter.ToInt32(header, 4);
    
                logger.LogInformation(
                    $"[{remoteEndPoint}] Stream ID {stream.Id}: Received Packet ID: {packetId}, Payload Length: {payloadLength}");
    
                if (payloadLength > 0)
                {
                    var payload = new byte[payloadLength];
    
                    if (await ReadExactAsync(stream, payload))
                    {
                        if (packetId is 0) // Connect
                        {
                            try
                            {
                                var connect = ConnectPacket.Deserialize(payload);
    
                                logger.LogInformation(
                                    "[{RemoteEndPoint}] Connect Packet: User={Username}, UUID={Uuid}, Type={ClientType}",
                                    remoteEndPoint, connect.Username, connect.Uuid, connect.ClientType);
                                logger.LogInformation("[{RemoteEndPoint}] Hash={ProtocolHash}", remoteEndPoint,
                                    connect.ProtocolHash);
    
                                // -- AUTH
                                await RequestAuthGrant(stream, connect);
    
                                // var passwordChallenge = GeneratePasswordChallengeIfNeeded(connect.Uuid);
                                //
                                // var connectAccept = new ConnectAccept
                                // {
                                //     PasswordChallenge = passwordChallenge
                                // };
                                //
                                // await SendPacketAsync(stream, connectAccept);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError("[{RemoteEndPoint}] Error deserializing Connect: {ExMessage}",
                                    remoteEndPoint, ex.Message);
                            }
    
                            logger.LogInformation("[{RemoteEndPoint}] Stream ID {StreamId}: Sending AuthGrant...",
                                remoteEndPoint, stream.Id);
    
                            // Try to send a fake JWT if no real token is available
                            // Header: {"alg":"HS256","typ":"JWT"} -> eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9
                            // Payload: {"sub":"1234567890","name":"LithiumServer","iat":1516239022} -> eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkxpdGhpdW1TZXJ2ZXIiLCJpYXQiOjE1MTYyMzkwMjJ9
                            // Signature: fake -> SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
    
                            // var grant = new AuthGrant
                            // {
                            //     AuthorizationGrant = "mock_auth_grant",
                            //     ServerIdentityToken = serverIdentityToken
                            // };
                            //
                            // await SendPacketAsync(stream, grant);
                        }
                        else if (packetId is 12) // AuthToken
                        {
                            logger.LogInformation(
                                "[{RemoteEndPoint}] Stream ID {StreamId}: Received AuthToken. Sending ServerAuthToken...",
                                remoteEndPoint, stream.Id);
    
                            // var passwordChallenge = GeneratePasswordChallengeIfNeeded()
    
                            // var serverAuthToken = new ServerAuthTokenPacket
                            // {
                            //     ServerAccessToken = serverAuthManager.Credentials.AccessToken,
                            //     PasswordChallenge = null // No server password
                            // };
    
                            var serverAuthToken =
                                new ServerAuthTokenPacket(serverAuthManager.Credentials?.AccessToken, null);
    
                            await SendPacketAsync<ServerAuthTokenPacket>(stream, serverAuthToken);
                        }
                        else
                        {
                            logger.LogInformation(
                                "[{RemoteEndPoint}] Stream ID {StreamId}: Payload (Hex): {BitToHexString}",
                                remoteEndPoint,
                                stream.Id, BitToHexString(payload));
                        }
                    }
                    else
                    {
                        logger.LogWarning("[{RemoteEndPoint}] Stream ID {StreamId}: Failed to read full payload.",
                            remoteEndPoint, stream.Id);
                        break; // Break loop on read error
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError("[{RemoteEndPoint}] Stream ID {StreamId} Error: {ExMessage}", remoteEndPoint, stream.Id,
                ex.Message);
        }
        finally
        {
            // Now we can safely dispose the stream as the loop has ended (EOF or Error)
            await stream.DisposeAsync();
            logger.LogInformation("[{RemoteEndPoint}] Stream ID {StreamId} disposed.", remoteEndPoint, stream.Id);
        }
    }

    private async Task RequestAuthGrant(QuicStream stream, ConnectPacket packet)
    {
        logger.LogInformation("Requesting authorization grant...");
    
        var clientIdentityToken = packet.IdentityToken;
        var serverSessionToken = serverAuthManager.GameSession?.SessionToken;
    
        if (!string.IsNullOrEmpty(serverSessionToken))
        {
            logger.LogInformation("Server session token available - requesting auth grant..");
    
            var serverAudience = AuthConstants.GetServerAudience(serverSessionToken);
            var authGrant =
                await sessionServiceClient.RequestAuthorizationGrantAsync(clientIdentityToken, serverAudience,
                    serverSessionToken);
    
            logger.LogInformation("Authorization grant obtained: {AuthGrant}", authGrant);
    
            if (string.IsNullOrEmpty(authGrant))
            {
                // Disconnect("Failed to obtain authorization grant from session service");
                logger.LogInformation("Failed to obtain authorization grant from session service");
            }
            else
            {
                var serverIdentityToken = serverAuthManager.GameSession?.IdentityToken;
    
                if (!string.IsNullOrEmpty(serverIdentityToken))
                {
                    // var authGrantPacket = new AuthGrantPacket
                    // {
                    //     AuthorizationGrant = authGrant,
                    //     ServerIdentityToken = serverIdentityToken
                    // };
    
                    var authGrantPacket = new AuthGrantPacket(authGrant, serverIdentityToken);
    
                    logger.LogInformation("Sending authorization grant to client...");
                    await SendPacketAsync<AuthGrantPacket>(stream, authGrantPacket);
                }
            }
        }
        else
        {
            logger.LogError("Server session token not available - cannot request auth grant");
            // Disconnect("Server authentication unavailable - please try again later");
        }
    }

    private static byte[]? GeneratePasswordChallengeIfNeeded(Guid playerUuid)
    {
        // TODO - This is the hardcoded password of the server
        var password = "PWD";

        if (!string.IsNullOrEmpty(password))
        {
            // if (Constants.SINGLEPLAYER) {
            //     UUID ownerUuid = SingleplayerModule.getUuid();
            //     if (ownerUuid != null && ownerUuid.equals(playerUuid)) {
            //         return null;
            //     }
            // }

            var challenge = new byte[32];
            new SecureRandom().NextBytes(challenge);

            return challenge;
        }

        return null;
    }

    private static async Task SendPacketAsync<T>(QuicStream stream, IPacket packet)
        where T : struct, IPacket<T>
    {
        using var ms = new MemoryStream();
        packet.Serialize(ms);

        var packetId = T.Id;
        var payload = ms.ToArray();

        using var fullPacket = new MemoryStream();
        PacketSerializer.WriteHeader(fullPacket, packetId, payload.Length);
        fullPacket.Write(payload);

        var data = fullPacket.ToArray();

        await stream.WriteAsync(data);
        await stream.FlushAsync();

        Console.WriteLine($"[Sent] {packet.GetType().Name} (ID {packetId}, Payload Length: {payload.Length})");
    }

    private static async Task<bool> ReadExactAsync(Stream stream, byte[] buffer)
    {
        var totalRead = 0;

        while (totalRead < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(totalRead, buffer.Length - totalRead));
            if (read is 0) return false;

            totalRead += read;
        }

        return true;
    }

    private static string BitToHexString(byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", " ");
    }

    // private async Task HandleStreamAsync(QuicConnection connection, CancellationToken ct)
    // {
    //     try
    //     {
    //         logger.LogInformation("Client connected: {client}", connection.RemoteEndPoint);
    //         
    //         var stream = await connection.AcceptInboundStreamAsync(ct);
    //         _connections[connection] = stream;
    //
    //         _ = Task.Run(() => packetHandler.HandleAsync(connection, stream), ct);
    //
    //         await KeepOpenAsync(ct);
    //     }
    //     catch (QuicException ex) when (ex.QuicError is QuicError.ConnectionAborted)
    //     {
    //         logger.LogInformation("[{ConnectionRemoteEndPoint}] Connection aborted.", connection.RemoteEndPoint);
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError("[{ConnectionRemoteEndPoint}] Error: {ExMessage}", connection.RemoteEndPoint, ex.Message);
    //     }
    //     finally
    //     {
    //         // Déconnexion propre comme dans Java
    //         // await SendDisconnectPacketAsync(connection, "Internal server error or closed by client", ct);
    //
    //         await connection.CloseAsync(0, ct);
    //         await connection.DisposeAsync();
    //
    //         _connections.TryRemove(connection, out _);
    //     }
    // }

    // private async Task SendDisconnectPacketAsync(QuicConnection connection, string message, CancellationToken ct)
    // {
    //     if (_connections.TryGetValue(connection, out var stream))
    //     {
    //         try
    //         {
    //             var packet = new DisconnectPacket(message, DisconnectType.Crash);
    //             var packetId = PacketRegistry.GetPacketId<DisconnectPacket>();
    //             var header = new PacketHeader(packetId, packet.GetSize());
    //             var data = PacketSerializer.SerializePacket(packet, header.TypeId);
    //
    //             await stream.WriteAsync(data, ct);
    //             await stream.FlushAsync(ct);
    //         }
    //         catch (Exception ex) when (ex is not OperationCanceledException)
    //         {
    //             logger.LogWarning(ex, "Failed to send disconnect packet");
    //         }
    //     }
    // }

    private static async Task KeepOpenAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(1000, ct);
        }
    }

    // private async Task HeartbeatLoopAsync(CancellationToken ct)
    // {
    //     try
    //     {
    //         while (!ct.IsCancellationRequested)
    //         {
    //             await Task.Delay(TimeSpan.FromSeconds(HeartbeatInterval), ct);
    //
    //             var ticks = DateTime.UtcNow.Ticks;
    //             var packet = new HeartbeatPacket(ticks);
    //             var packetId = PacketRegistry.GetPacketId<HeartbeatPacket>();
    //             var header = new PacketHeader(packetId, packet.GetSize());
    //             var data = PacketSerializer.SerializePacket(packet, header.TypeId);
    //
    //             foreach (var kv in _connections)
    //             {
    //                 var stream = kv.Value;
    //
    //                 try
    //                 {
    //                     await stream.WriteAsync(data, ct);
    //                     await stream.FlushAsync(ct);
    //                 }
    //                 catch (Exception ex) when (ex is not OperationCanceledException)
    //                 {
    //                     logger.LogWarning(ex, "Failed to send heartbeat");
    //                     break;
    //                 }
    //             }
    //
    //             await hub.Clients.All.Heartbeat(ticks);
    //             logger.LogInformation("Heartbeat sent to all clients");
    //         }
    //     }
    //     catch (OperationCanceledException)
    //     {
    //         logger.LogInformation("Heartbeat loop was cancelled");
    //         throw;
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Error in heartbeat loop");
    //         throw;
    //     }
    // }

    public async ValueTask DisposeAsync()
    {
        foreach (var (connection, channel) in _channels)
        {
            await channel.CloseAsync();
            await connection.CloseAsync(0);
            await connection.DisposeAsync();
        }

        await _listener.DisposeAsync();
    }
}