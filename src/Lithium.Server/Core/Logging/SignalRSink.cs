using Serilog.Core;
using Serilog.Events;

namespace Lithium.Server.Core.Logging;

public sealed class SignalRSink : ILogEventSink
{
    public static event Action<LogEvent>? LogEmitted;

    public void Emit(LogEvent logEvent)
    {
        LogEmitted?.Invoke(logEvent);
    }
}