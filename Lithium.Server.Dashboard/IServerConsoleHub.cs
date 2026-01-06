namespace Lithium.Server.Dashboard;

public interface IServerConsoleHub
{
    Task ReceiveCommandSuggestions(string[] commands);
    Task ReceiveLog(DateTimeOffset timestamp, int level, string message);
}