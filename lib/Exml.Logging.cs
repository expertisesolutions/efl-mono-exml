using System;

namespace Exml
{
namespace Logging
{

public interface ILogger
{
    void Log(string message);
}

public class Logger
{
    private static ILogger logger;

    public static void Info(string msg)
    {
        if (logger == null)
            return; // FIXME do something here
        logger.Log(msg);
    }

    public static void SetLogger(ILogger logger)
    {
        Logger.logger = logger;
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
