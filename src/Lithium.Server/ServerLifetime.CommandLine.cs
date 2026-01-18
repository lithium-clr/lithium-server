using System.CommandLine;
using Lithium.Server.Core;

namespace Lithium.Server;

public partial class ServerLifetime : IServerCommandLineProvider
{
    private readonly RootCommand _rootCommand = new();
    private ParseResult _commands = null!;
    
    public Option<bool> IsSinglePlayerOption { get; } = new("--single-player")
    {
        Description = "Run in single player mode"
    };
        
    public Option<string> OwnerUuidOption { get; } = new("--owner-uuid")
    {
        Description = "The owner's UUID",
        Arity = ArgumentArity.ZeroOrOne
    };
    
    public Option<string> OwnerNameOption { get; } = new("--owner-name")
    {
        Description = "The owner's name"
    };
    
    public Option<string> SessionTokenOption { get; } = new("--session-token")
    {
        Description = "The session token to use"
    };
    
    public Option<string> IdentityTokenOption { get; } = new("--identity-token")
    {
        Description = "The identity token to use"
    };

    private void RegisterCommandLines()
    {
        _rootCommand.Add(IsSinglePlayerOption);
        _rootCommand.Add(OwnerUuidOption);
        _rootCommand.Add(OwnerNameOption);
        _rootCommand.Add(SessionTokenOption);
        _rootCommand.Add(IdentityTokenOption);
        
        _rootCommand.Description = "Server commands";

        var args = Environment.GetCommandLineArgs();
        
        // Skip the first argument which is the executable path, as System.CommandLine expects only arguments
        if (args.Length > 0)
             args = args.Skip(1).ToArray();
        
        _commands = _rootCommand.Parse(args);
        if (_commands.Errors.Count is 0) return;
        
        logger.LogError($"Error when parsing command line arguments ({_commands.Errors.Count}):");
            
        foreach (var parseError in _commands.Errors)
            logger.LogError("Parser Error: " + parseError.Message);
    }
}