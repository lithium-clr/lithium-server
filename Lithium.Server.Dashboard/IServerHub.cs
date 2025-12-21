namespace Lithium.Server.Dashboard;

public interface IServerHub
{
    Task Heartbeat(long ticks);
}