using System.CommandLine;

namespace Lithium.Server.Core;

public interface IServerCommandLineProvider
{
    Option<bool> IsSinglePlayerOption { get; }
    Option<string> OwnerUuidOption { get; }
}