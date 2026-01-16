using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Auth.OAuth;

public sealed class OAuthClient(HttpClient httpClient, ILogger<OAuthClient> logger)
{
    public async Task StartFlow(OAuthBrowserFlow flow, CancellationTokenSource cts)
    {
        _ = Task.Run(async () =>
        {
            HttpListener? listener = null;

            try
            {
                var state = GenerateRandomString(32);
                var verifier = GenerateRandomString(64);
                var challenge = GenerateCodeChallenge(verifier);

                var port = GetFreePort();
                const string redirectUri = AuthConstants.ConsentRedirectUrl;

                listener = new HttpListener();
                listener.Prefixes.Add($"http://127.0.0.1:{port}/");
                listener.Start();

                var authCodeTcs = new TaskCompletionSource<string>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

                _ = HandleBrowserCallbackAsync(listener, state, authCodeTcs);

                var authUrl = BuildAuthUrl(state, challenge, redirectUri);
                flow.OnFlowInfo(authUrl);

                logger.LogInformation("Waiting for code...");
                
                var code = await authCodeTcs.Task
                    .WaitAsync(TimeSpan.FromMinutes(5), cts.Token);
                
                logger.LogInformation("Authentication code received: " + code);

                if (cts.IsCancellationRequested)
                {
                    flow.OnFailure("Authentication cancelled");
                    return;
                }

                var tokens = await ExchangeCodeForTokensAsync(code, verifier, redirectUri, cts.Token);

                if (tokens is null)
                {
                    flow.OnFailure("Token exchange failed");
                    return;
                }

                logger.LogInformation("Token exchange successful: " + tokens.AccessToken);
                flow.OnSuccess(tokens);
            }
            catch (OperationCanceledException)
            {
                flow.OnFailure("Authentication cancelled");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "OAuth browser flow failed");
                flow.OnFailure(ex.Message);
            }
            finally
            {
                listener?.Close();
            }
        }, cts.Token);
    }
    
    public async Task StartFlow(OAuthDeviceFlow flow, CancellationTokenSource cts)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var deviceAuth = await RequestDeviceAuthorizationAsync(cts.Token);
    
                if (deviceAuth is null)
                {
                    flow.OnFailure("Failed to start device authorization");
                    return;
                }
    
                flow.OnFlowInfo(
                    deviceAuth.UserCode,
                    deviceAuth.VerificationUri,
                    deviceAuth.VerificationUriComplete,
                    deviceAuth.ExpiresIn);
    
                var pollInterval = Math.Max(deviceAuth.Interval, 5);
                var deadline = DateTimeOffset.UtcNow.AddSeconds(deviceAuth.ExpiresIn);
    
                while (DateTimeOffset.UtcNow < deadline && !cts.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(pollInterval), cts.Token);
    
                    var token = await PollDeviceTokenAsync(deviceAuth.DeviceCode, cts.Token);
                    if (token is null) continue;
    
                    if (token.IsSuccess())
                    {
                        flow.OnSuccess(token);
                        return;
                    }
    
                    if (token.Error is "slow_down")
                        pollInterval += 5;
    
                    else if (token.Error is not "authorization_pending")
                    {
                        flow.OnFailure($"Device auth failed: {token.Error}");
                        return;
                    }
                }
    
                flow.OnFailure(cts.IsCancellationRequested
                    ? "Authentication cancelled"
                    : "Device authorization expired");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "OAuth device flow failed");
                flow.OnFailure(ex.Message);
            }
        }, cts.Token);
    }

    private static async Task HandleBrowserCallbackAsync(
        HttpListener listener,
        string expectedState,
        TaskCompletionSource<string> tcs
    )
    {
        var context = await listener.GetContextAsync();
        var query = context.Request.Url!.Query;

        var code = ExtractQueryParam(query, "code");
        var state = ExtractQueryParam(query, "state");

        string html;
        int status;

        if (state != expectedState)
        {
            html = BuildHtmlPage(
                false,
                "Authentication Failed",
                "Authentication Failed",
                "Something went wrong during authentication. Please close this window and try again.",
                "Invalid state parameter"
            );
            status = 400;
            tcs.TrySetException(new InvalidOperationException("Invalid state"));
        }
        else if (!string.IsNullOrEmpty(code))
        {
            html = BuildHtmlPage(
                true,
                "Authentication Successful",
                "Authentication Successful",
                "You have been logged in successfully. You can now close this window and return to the server.",
                null
            );
            status = 200;
            tcs.TrySetResult(code);
        }
        else
        {
            var errorMessage = ExtractQueryParam(query, "error") ?? "No code received";

            html = BuildHtmlPage(
                false,
                "Authentication Failed",
                "Authentication Failed",
                "Something went wrong during authentication. Please close this window and try again.",
                errorMessage
            );
            status = 400;
            tcs.TrySetException(new InvalidOperationException(errorMessage));
        }

        context.Response.StatusCode = status;
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(html));
        context.Response.Close();
    }

    public async ValueTask<TokenResponse?> RefreshTokensAsync(
        string refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                AuthConstants.OauthTokenUrl
            );

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = AuthConstants.ClientId,
                ["refresh_token"] = refreshToken
            });

            request.Headers.UserAgent.ParseAdd(AuthConstants.UserAgent);
            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                logger.LogWarning(
                    "Token refresh failed: HTTP {StatusCode} - {Body}",
                    response.StatusCode,
                    body);

                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseTokenResponse(json);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Token refresh request was canceled");
            throw;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "HTTP error during token refresh");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Token refresh failed");
            return null;
        }
    }

    private async ValueTask<TokenResponse?> ExchangeCodeForTokensAsync(
        string code,
        string codeVerifier,
        string redirectUri,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(codeVerifier);
        ArgumentException.ThrowIfNullOrWhiteSpace(redirectUri);

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                AuthConstants.OauthTokenUrl
            );

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = AuthConstants.ClientId,
                ["code"] = code,
                ["redirect_uri"] = redirectUri,
                ["code_verifier"] = codeVerifier
            });

            request.Headers.UserAgent.ParseAdd(AuthConstants.UserAgent);
            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                logger.LogWarning(
                    "Token exchange failed: HTTP {StatusCode} - {Body}",
                    response.StatusCode,
                    body);

                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseTokenResponse(json);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Token exchange request was canceled");
            throw;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "HTTP error during token exchange");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Token exchange failed");
            return null;
        }
    }

    private async ValueTask<DeviceAuthResponse?> RequestDeviceAuthorizationAsync(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                AuthConstants.DeviceAuthUrl
            );
    
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = AuthConstants.ClientId,
                ["scope"] = string.Join(' ', AuthConstants.Scopes)
            });
    
            request.Headers.UserAgent.ParseAdd(AuthConstants.UserAgent);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    
            using var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
    
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
    
                logger.LogWarning(
                    "Device authorization request failed: HTTP {StatusCode} - {Body}",
                    response.StatusCode,
                    body);
    
                return null;
            }
    
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseDeviceAuthResponse(json);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Device authorization request was canceled");
            throw;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "HTTP error during device authorization request");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Device authorization request failed");
            return null;
        }
    }
    
    private async ValueTask<TokenResponse?> PollDeviceTokenAsync(
        string deviceCode,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceCode);
    
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                AuthConstants.OauthTokenUrl
            );
    
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code",
                ["client_id"] = AuthConstants.ClientId,
                ["device_code"] = deviceCode
            });
    
            request.Headers.UserAgent.ParseAdd(AuthConstants.UserAgent);
            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
    
            using var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );
    
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
    
            if (response.StatusCode is HttpStatusCode.BadRequest)
            {
                // Comme le Java : code 400 → parse token response
                return ParseTokenResponse(body);
            }
    
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Device token poll failed: HTTP {StatusCode} - {Body}",
                    response.StatusCode,
                    body);
    
                return null;
            }
    
            return ParseTokenResponse(body);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Device token poll was canceled");
            throw;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "HTTP error during device token poll");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Device token poll failed");
            return null;
        }
    }

    private static TokenResponse? ParseTokenResponse(string json)
    {
        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<TokenResponse>(json);
    }

    private static DeviceAuthResponse? ParseDeviceAuthResponse(string json)
    {
        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<DeviceAuthResponse>(json);
    }

    private static string BuildAuthUrl(string state, string challenge, string redirectUri) =>
        AuthConstants.OauthAuthUrl +
        $"?response_type=code&client_id=" + AuthConstants.ClientId +
        $"&redirect_uri={Url(redirectUri)}" +
        $"&scope={Url(string.Join(' ', AuthConstants.Scopes))}" +
        $"&state={Url(state)}" +
        $"&code_challenge={Url(challenge)}&code_challenge_method=S256";

    private static string GenerateRandomString(int length)
    {
        Span<byte> bytes = stackalloc byte[length];
        RandomNumberGenerator.Fill(bytes);
        return Base64Url(bytes)[..length];
    }

    private static string GenerateCodeChallenge(string verifier) =>
        Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));

    private static string Base64Url(ReadOnlySpan<byte> data) =>
        Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static string Url(string value) =>
        WebUtility.UrlEncode(value);

    private static string? ExtractQueryParam(string query, string name)
    {
        var match = Regex.Match(query, $"{name}=([^&]+)");
        return match.Success ? WebUtility.UrlDecode(match.Groups[1].Value) : null;
    }

    private static int GetFreePort()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        var port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }

    private static string BuildHtmlPage(
        bool success,
        string title,
        string heading,
        string message,
        string? errorDetail)
    {
        var detail = !string.IsNullOrEmpty(errorDetail)
            ? $"<div class=\"error\">{WebUtility.HtmlEncode(errorDetail)}</div>"
            : string.Empty;

        var iconClass = success ? "icon-success" : "icon-error";

        var iconSvg = success
            ? "<polyline points=\"20 6 9 17 4 12\"></polyline>"
            : "<line x1=\"18\" y1=\"6\" x2=\"6\" y2=\"18\"></line><line x1=\"6\" y1=\"6\" x2=\"18\" y2=\"18\"></line>";

        return $$"""
                 <!DOCTYPE html>
                 <html lang="en">
                 <head>
                     <meta charset="UTF-8">
                     <meta name="viewport" content="width=device-width, initial-scale=1.0">
                     <title>{{title}} - Hytale</title>
                     <link rel="preconnect" href="https://fonts.googleapis.com">
                     <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
                     <link href="https://fonts.googleapis.com/css2?family=Lexend:wght@700&family=Nunito+Sans:wght@400;700&display=swap" rel="stylesheet">
                     <style>
                         * { margin: 0; padding: 0; box-sizing: border-box; }
                         html { color-scheme: dark; background: linear-gradient(180deg, #15243A, #0F1418); min-height: 100vh; }
                         body { font-family: "Nunito Sans", sans-serif; color: #b7cedd; min-height: 100vh; display: flex; align-items: center; justify-content: center; padding: 20px; }
                         .card { background: rgba(0,0,0,0.4); border: 2px solid rgba(71,81,107,0.6); border-radius: 12px; padding: 48px 40px; max-width: 420px; text-align: center; }
                         .icon { width: 64px; height: 64px; margin: 0 auto 24px; border-radius: 50%; display: flex; align-items: center; justify-content: center; }
                         .icon svg { width: 32px; height: 32px; }
                         .icon-success { background: linear-gradient(135deg, #2d5a3d, #1e3a2a); border: 2px solid #4a9d6b; }
                         .icon-success svg { color: #6fcf97; }
                         .icon-error { background: linear-gradient(135deg, #5a2d3d, #3a1e2a); border: 2px solid #c3194c; }
                         .icon-error svg { color: #ff6b8a; }
                         h1 { font-family: "Lexend", sans-serif; font-size: 1.5rem; text-transform: uppercase; background: linear-gradient(#f5fbff, #bfe6ff); -webkit-background-clip: text; background-clip: text; color: transparent; margin-bottom: 12px; }
                         p { line-height: 1.6; }
                         .error { background: rgba(195,25,76,0.15); border: 1px solid rgba(195,25,76,0.4); border-radius: 6px; padding: 12px; margin-top: 16px; color: #ff8fa8; font-size: 0.875rem; word-break: break-word; }
                     </style>
                 </head>
                 <body>
                     <div class="card">
                         <div class="icon {{iconClass}}">
                             <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                                 {{iconSvg}}
                             </svg>
                         </div>
                         <h1>{{heading}}</h1>
                         <p>{{message}}</p>
                         {{detail}}
                     </div>
                 </body>
                 </html>
                 """;
    }
}