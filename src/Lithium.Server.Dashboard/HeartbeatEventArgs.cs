namespace Lithium.Server.Dashboard;

public sealed class HeartbeatEventArgs(long ticks) : EventArgs
{
    public readonly long Ticks = ticks;
}