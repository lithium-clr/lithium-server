namespace Lithium.Server.Core.Auth;

public sealed record DeviceAuthResponse(
    string DeviceCode,
    string UserCode,
    string VerificationUri,
    string VerificationUriComplete,
    int ExpiresIn,
    int Interval
);