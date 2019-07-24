using System;
using System.Runtime.CompilerServices;

namespace Exml
{
namespace Logging
{

public interface ILogger
{
    void Log(string message);
}

public enum LogLevel
{
    Critical = 0,
    Error = 1,
    Warning = 2,
    Info = 3,
    Debug = 4,
}

public class Logger
{
    private static ILogger logger;
    public static LogLevel Level { get; private set; }

    public static void Critical(string msg,
                                [CallerLineNumber] int line = 0,
                                [CallerFilePath] string file = null,
                                [CallerMemberName] string member = null)
    {
        Log(LogLevel.Critical, msg, line, file, member);
    }

    public static void Error(string msg,
                             [CallerLineNumber] int line = 0,
                             [CallerFilePath] string file = null,
                             [CallerMemberName] string member = null)
    {
        Log(LogLevel.Error, msg, line, file, member);
    }

    public static void Warning(string msg,
                               [CallerLineNumber] int line = 0,
                               [CallerFilePath] string file = null,
                               [CallerMemberName] string member = null)
    {
        Log(LogLevel.Warning, msg, line, file, member);
    }

    public static void Info(string msg,
                            [CallerLineNumber] int line = 0,
                            [CallerFilePath] string file = null,
                            [CallerMemberName] string member = null)
    {
        Log(LogLevel.Info, msg, line, file, member);
    }

    public static void Debug(string msg,
                             [CallerLineNumber] int line = 0,
                             [CallerFilePath] string file = null,
                             [CallerMemberName] string member = null)
    {
        Log(LogLevel.Debug, msg, line, file, member);
    }

    private static void Log(LogLevel targetLevel, string msg,
                            int line, string file, string member)
    {
        if (logger == null)
        {
            return; // FIXME do something here
        }

        if (Level >= targetLevel)
        {
            logger.Log($"{file}:{line}::{member} ({Level}): {msg}");
        }
    }

    public static void SetLogger(ILogger logger)
    {
        Logger.logger = logger;
    }

    public static void SetLevel(LogLevel level)
    {
        Level = level;
    }

    public static void SetLevelFromEnvironment(LogLevel fallback=LogLevel.Warning)
    {
        var required = Environment.GetEnvironmentVariable("EXML_LOG_LEVEL");
        var level = fallback;

        if (required != null)
        {
            if (required == "critical")
            {
                level = LogLevel.Critical;
            }
            else if (required == "error")
            {
                level = LogLevel.Error;
            }
            else if (required == "warning")
            {
                level = LogLevel.Warning;
            }
            else if (required == "info")
            {
                level = LogLevel.Info;
            }
            else if (required == "debug")
            {
                level = LogLevel.Debug;
            }
            else
            {
                throw new ArgumentException("Invalid log level value. Must be critical,error,warning,info or debug");
            }
        }

        SetLevel(level);
    }

    public static void AddConsoleLogger()
    {
        SetLogger(new ConsoleLogger());
    }

    private class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}

}
}
