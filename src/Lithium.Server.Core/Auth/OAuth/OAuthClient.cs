using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Auth.OAuth;

public sealed class OAuthClient(HttpClient httpClient, ILogger<OAuthClient> logger)
{
    public async Task<OAuthFlowResult> StartFlowAsync(IOAuthDeviceFlow flow, CancellationToken cancellationToken)
    {
        try
        {
            var deviceAuth = await RequestDeviceAuthorizationAsync(cancellationToken);

            if (deviceAuth is null)
            {
                return OAuthFlowResult.Failed("Failed to start device authorization");
            }

            flow.OnFlowInfo(
                deviceAuth.UserCode,
                deviceAuth.VerificationUri,
                deviceAuth.VerificationUriComplete,
                deviceAuth.ExpiresIn);

            var pollInterval = Math.Max(deviceAuth.Interval, 5);
            var deadline = DateTimeOffset.UtcNow.AddSeconds(deviceAuth.ExpiresIn);

            while (DateTimeOffset.UtcNow < deadline && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(pollInterval), cancellationToken);

                var token = await PollDeviceTokenAsync(deviceAuth.DeviceCode, cancellationToken);
                if (token is null) continue;

                if (token.IsSuccess())
                {
                    return OAuthFlowResult.Success(token);
                }

                if (token.Error is "slow_down")
                    pollInterval += 5;

                else if (token.Error is not "authorization_pending")
                {
                    return OAuthFlowResult.Failed($"Device auth failed: {token.Error}");
                }
            }

            return OAuthFlowResult.Failed(cancellationToken.IsCancellationRequested
                ? "Authentication cancelled"
                : "Device authorization expired");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OAuth device flow failed");
            return OAuthFlowResult.Failed(ex.Message);
        }
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
}