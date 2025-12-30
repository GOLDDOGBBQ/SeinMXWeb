using System;
using Microsoft.Extensions.Logging;

namespace SEINMX.Clases;

public class CargoBajaLibLoggerAdapter : CargoBajaLib.ILogger
{
    private readonly ILogger _microsoftLogger;

    public CargoBajaLibLoggerAdapter(ILogger microsoftLogger)
    {
        _microsoftLogger = microsoftLogger;
    }

    public void LogTrace(Exception exception, string message, params object[] args)
    {
        _microsoftLogger.LogTrace(exception, message, args);
    }

    public void LogDebug(Exception exception, string message, params object[] args)
    {
        _microsoftLogger.LogDebug(exception, message, args);
    }

    public void LogInformation(Exception exception, string message, params object[] args)
    {
        _microsoftLogger.LogInformation(exception, message, args);
    }

    public void LogWarning(Exception exception, string message, params object[] args)
    {
        _microsoftLogger.LogWarning(exception, message, args);
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        _microsoftLogger.LogError(exception, message, args);
    }

    public void LogCritical(Exception exception, string message, params object[] args)
    {
        _microsoftLogger.LogCritical(exception, message, args);
    }
}

public static class LoggerExtensions
{
    public static CargoBajaLib.ILogger ToCargoBajaLibLogger(this ILogger microsoftLogger)
    {
        return new CargoBajaLibLoggerAdapter(microsoftLogger);
    }
}