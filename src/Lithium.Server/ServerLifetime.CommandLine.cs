using System.CommandLine;
using Lithium.Server.Core.Auth;

namespace Lithium.Server;

public partial class ServerLifetime : IServerCommandLineProvider
{
    private readonly RootCommand _rootCommand = new();
    
    public Option<bool> IsSinglePlayerOption => new("--single-player")
    {
        Description = "Run in single player mode"
    };
        
    public Option<Guid> OwnerUuidOption => new("--owner-uuid")
    {
        Description = "The owner's UUID"
    };

    private void RegisterCommandLines()
    {
        _rootCommand.Options.Add(IsSinglePlayerOption);
        _rootCommand.Options.Add(OwnerUuidOption);
        
        _rootCommand.Description = "Server commands";

        var args = Environment.GetCommandLineArgs();
        
        var commands = _rootCommand.Parse(args);
        if (commands.Errors.Count is 0) return;
        
        logger.LogError("Error when parsing command line arguments:");
            
        foreach (var parseError in commands.Errors)
            logger.LogError(parseError.Message);
    }
}