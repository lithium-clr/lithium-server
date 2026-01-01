namespace Lithium.Server.Core.Systems.Commands;

public static class ILoggerExtensions
{
    public delegate void SentryLogDelegate(
        Action<SentryLog> configureLog,
        string template,
        object[] parameters
    );

    public static void SendToSentry<T>(
        ILogger<T> logger,
        SentryLogDelegate sentryLog,
        LogLevel level,
        string message,
        object[] args,
        Exception? ex = null
    )
    {
        sentryLog(
            log => log.SetAttribute("force_send", "true"),
            message,
            args
        );

        if (ex is null)
            logger.Log(level, message, args);
        else
            logger.Log(level, ex, message, args);
    }

    extension<T>(ILogger<T> logger)
    {
        public void SentryTrace(string message, params object[] args) =>
            SendToSentry(logger, SentrySdk.Logger.LogTrace, LogLevel.Trace, message, args);

        public void SentryDebug(string message, params object[] args) =>
            SendToSentry(logger, SentrySdk.Logger.LogDebug, LogLevel.Debug, message, args);

        public void SentryInfo(string message, params object[] args) =>
            SendToSentry(logger, SentrySdk.Logger.LogInfo, LogLevel.Information, message, args);

        public void SentryWarning(string message, params object[] args) =>
            SendToSentry(logger, SentrySdk.Logger.LogWarning, LogLevel.Warning, message, args);

        public void SentryError(string message, params object[] args) =>
            SendToSentry(logger, SentrySdk.Logger.LogError, LogLevel.Error, message, args);

        public void SentryFatal(string message, params object[] args) =>
            SendToSentry(logger, SentrySdk.Logger.LogFatal, LogLevel.Critical, message, args);

        public void SentryError(Exception ex, string message, params object[] args) =>
            SendToSentry(logger, SentrySdk.Logger.LogError, LogLevel.Error, message, args, ex);

        public void SentryFatal(Exception ex, string message, params object[] args) =>
            SendToSentry(logger, SentrySdk.Logger.LogFatal, LogLevel.Critical, message, args, ex);

        public void SentryWarning(Exception ex, string message, params object[] args) =>
            SendToSentry(logger, SentrySdk.Logger.LogWarning, LogLevel.Warning, message, args, ex);

        public void SentryInfo(Exception ex, string message, params object[] args) =>
            SendToSentry(logger, SentrySdk.Logger.LogInfo, LogLevel.Information, message, args, ex);

        public void SentryDebug(Exception ex, string message, params object[] args) =>
            SendToSentry(logger, SentrySdk.Logger.LogDebug, LogLevel.Debug, message, args, ex);
    }
}