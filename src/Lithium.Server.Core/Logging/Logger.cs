using Microsoft.Extensions.Logging;

namespace Lithium.Server.Core.Logging;

public interface ILoggerService;

public sealed class LoggerService(ILoggerFactory loggerFactory) : ILoggerService
{
    internal void Init()
    {
        Logger.Init(loggerFactory);
    }
}

public sealed class Logger
{
    private static ILogger _logger = null!;
    private static ILoggerFactory _loggerFactory = null!;

    public string Name { get; }

    internal Logger(string name)
    {
        if (_loggerFactory is null)
            throw new InvalidOperationException("LoggerFactory not initialized");

        _logger = _loggerFactory.CreateLogger(name);

        Name = name;
    }

    internal static void Init(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public void Trace(object message)
    {
        _logger.LogTrace(message.ToString());
    }

    public void Trace(FormattableString message)
    {
        _logger.LogTrace(message.Format, message.GetArguments());
    }

    public void Trace(Exception exception)
    {
        _logger.LogTrace(exception, exception.Message);
    }

    public void Debug(object message)
    {
        _logger.LogDebug(message.ToString());
    }

    public void Debug(FormattableString message)
    {
#if DEBUG
        _logger.LogDebug(message.Format, message.GetArguments());
#endif
    }

    public void Debug(Exception exception)
    {
        _logger.LogDebug(exception, exception.Message);
    }

    public void Info(object message)
    {
        _logger.LogInformation(message.ToString());
    }

    public void Info(FormattableString message)
    {
        _logger.LogInformation(message.Format, message.GetArguments());
    }

    public void Warning(object message)
    {
        _logger.LogWarning(message.ToString());
    }

    public void Warning(FormattableString message)
    {
        _logger.LogWarning(message.Format, message.GetArguments());
    }

    public void Error(object message)
    {
        _logger.LogError(message.ToString());
    }

    public void Error(FormattableString message)
    {
        _logger.LogError(message.Format, message.GetArguments());
    }

    public void Error(Exception exception)
    {
        _logger.LogError(exception, exception.Message);
    }
}