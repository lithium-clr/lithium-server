namespace Lithium.Server.Core.Networking.Authentication;

public enum AuthState
{
    RequestingAuthGrant,
    AwaitingAuthToken,
    ProcessingAuthToken,
    ExchangingServerToken,
    Authenticated
}