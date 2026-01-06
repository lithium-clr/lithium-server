namespace Lithium.Server.Dashboard;

public static class LogLevelExtensions
{
    public static string ToShortName(this LogLevel level) => level switch
    {
        LogLevel.Debug => "DBG",
        LogLevel.Trace => "TRA",
        LogLevel.Information => "INF",
        LogLevel.Warning => "WRN",
        LogLevel.Error => "ERR",
        LogLevel.Critical => "CRT",
        _ => ""
    };
}