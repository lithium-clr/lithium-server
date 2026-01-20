namespace Lithium.Server.Core.Protocol;

public sealed record ShutdownReason
{
    public Reasons ExitCode { get; }
    public string? Message { get; }

    public ShutdownReason(Reasons exitCode)
    {
        ExitCode = exitCode;
        Message = null;
    }

    public ShutdownReason(Reasons exitCode, string message)
    {
        ExitCode = exitCode;
        Message = message;
    }

    public ShutdownReason WithMessage(string message)
    {
        return new ShutdownReason(ExitCode, message);
    }

    public override string ToString()
    {
        return "ShutdownReason{exitCode=" + ExitCode + ", message='" + Message + "'}";
    }

    public enum Reasons : byte
    {
        Shutdown = 0,
        Crash = 1,
        AuthFailed = 2,
        WorldGen = 3,
        ClientGone = 4,
        MissingRequiredPlugin = 5,
        ValidateError = 6,
        MissingAssets = 7,
        SigInt = 130,
    }
}