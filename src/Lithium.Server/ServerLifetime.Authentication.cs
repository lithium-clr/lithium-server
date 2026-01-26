using Lithium.Server.Core.Networking.Authentication;
using Lithium.Server.Core.Systems.Commands;

namespace Lithium.Server;

public partial class ServerLifetime
{
    private ServerAuthManager.ServerAuthContext SetupAuthenticationContext()
    {
        bool isSinglePlayer;
        Guid? ownerUuid = null;
        string? ownerName;
        string? sessionToken;
        string? identityToken;

        if (_commands.GetValue(IsSinglePlayerOption) is var singlePlayer)
            isSinglePlayer = singlePlayer;

        if (_commands.GetValue(OwnerUuidOption) is var ownerUuidString &&
            Guid.TryParse(ownerUuidString, out var parsedUuid))
            ownerUuid = parsedUuid;

        if (_commands.GetValue(OwnerNameOption) is var ownerNameString)
            ownerName = ownerNameString;

        if (_commands.GetValue(SessionTokenOption) is var sessionTokenString)
            sessionToken = sessionTokenString;

        if (_commands.GetValue(IdentityTokenOption) is var identityTokenString)
            identityToken = identityTokenString;

        return new ServerAuthManager.ServerAuthContext
        {
            IsSinglePlayer = isSinglePlayer,
            OwnerUuid = ownerUuid,
            OwnerName = ownerName,
            SessionToken = sessionToken,
            IdentityToken = identityToken
        };
    }
    
    private async Task EnsureAuthenticationAsync()
    {
        logger.LogInformation("Ensuring authentication...");
        
        if (serverAuthManager.IsSinglePlayer)
        {
            logger.LogInformation("Single player selected");
        }
        else if (!string.IsNullOrEmpty(serverAuthManager.SessionToken) &&
                 !string.IsNullOrEmpty(serverAuthManager.IdentityToken))
        {
            logger.LogInformation("Already authenticated");
        }
        else
        {
            await RequestAuthenticationAsync();
        }
    }
    
    private async Task RequestAuthenticationAsync()
    {
        var authResult = await serverAuthManager.StartFlowAsync(deviceFlow, new CancellationTokenSource());

        switch (authResult)
        {
            case AuthResult.Success:
                logger.LogInformation("Authentication successful");
                break;
            case AuthResult.PendingProfileSelection:
                logger.LogInformation("Profile selection required");

                var profiles = serverAuthManager.PendingProfiles;

                foreach (var profile in profiles)
                    logger.LogInformation("{Username} ({Uuid})", profile.Username, profile.Uuid);

                break;
            case AuthResult.Failed:
                logger.LogInformation("Authentication failed");
                break;
        }
    }
    
    [ConsoleCommand("auth")]
    public async Task AuthCommand(string command, string loginType)
    {
        switch (command)
        {
            case "login":
                switch (loginType)
                {
                    case "browser":
                        throw new NotImplementedException();
                    case "device":
                        await EnsureAuthenticationAsync();
                        break;
                }

                break;
        }
    }
}