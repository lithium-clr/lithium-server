namespace Lithium.Server.Core.Systems.Commands;

public static class SentrySdkExtensions
{
    extension(SentrySdk)
    {
        public static void LogTrace(string message, params object[] args) =>
            SentrySdk.Logger.LogTrace(message, args);
        
        public static void LogDebug(string message, params object[] args) =>
            SentrySdk.Logger.LogDebug(message, args);
        
        public static void LogInfo(string message, params object[] args) =>
            SentrySdk.Logger.LogInfo(message, args);
        
        public static void LogWarning(string message, params object[] args) =>
            SentrySdk.Logger.LogWarning(message, args);
        
        public static void LogError(string message, params object[] args) =>
            SentrySdk.Logger.LogError(message, args);
        
        public static void LogFatal(string message, params object[] args) =>
            SentrySdk.Logger.LogFatal(message, args);
    }
}