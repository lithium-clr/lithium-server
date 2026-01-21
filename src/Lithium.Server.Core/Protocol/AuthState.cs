namespace Lithium.Server.Core.Protocol;

public enum AuthState
{
    RequestingAuthGrant,
    AwaitingAuthToken,
    ProcessingAuthToken,
    ExchangingServerToken,
    Authenticated
}