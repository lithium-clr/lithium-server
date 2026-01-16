using System.CommandLine;
using Lithium.Server.Core;
using Lithium.Server.Core.Systems.Commands;

namespace Lithium.Server;

public partial class ServerLifetime : IServerCommandLineProvider
{
    private readonly RootCommand _rootCommand = new();
    private ParseResult _commands = null!;
    
    public Option<bool> IsSinglePlayerOption => new("--single-player")
    {
        Description = "Run in single player mode"
    };
        
    public Option<Guid> OwnerUuidOption => new("--owner-uuid")
    {
        Description = "The owner's UUID"
    };
    
    public Option<string> OwnerNameOption => new("--owner-name")
    {
        Description = "The owner's name"
    };
    
    public Option<string> SessionTokenOption => new("--session-token")
    {
        Description = "The session token to use"
    };
    
    public Option<string> IdentityTokenOption => new("--identity-token")
    {
        Description = "The identity token to use"
    };

    private void RegisterCommandLines()
    {
        _rootCommand.Options.Add(IsSinglePlayerOption);
        _rootCommand.Options.Add(OwnerUuidOption);
        _rootCommand.Options.Add(OwnerNameOption);
        _rootCommand.Options.Add(SessionTokenOption);
        _rootCommand.Options.Add(IdentityTokenOption);
        
        _rootCommand.Description = "Server commands";

        var args = Environment.GetCommandLineArgs();
        
        _commands = _rootCommand.Parse(args);
        if (_commands.Errors.Count is 0) return;
        
        logger.LogError("Error when parsing command line arguments:");
            
        foreach (var parseError in _commands.Errors)
            logger.LogError(parseError.Message);
    }
}