using System.CommandLine;

namespace Lithium.Server.Core.Auth;

public interface IServerCommandLineProvider
{
    Option<bool> IsSinglePlayerOption { get; }
    Option<Guid> OwnerUuidOption { get; }
}