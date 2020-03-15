using NLog;
using System;

namespace CBS.Siren.Logging
{
    public static class LoggingManager
    {
        public static void ConfigureLogging()
        {
            var config = new NLog.Config.LoggingConfiguration();

            var logConsole = new NLog.Targets.ConsoleTarget("console");

            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logConsole);

            NLog.LogManager.Configuration = config;
        }

        public static ILogger GetLogger(string loggerName)
        {
            return NLog.LogManager.GetLogger(loggerName);
        }

        public static void Shutdown()
        {
            NLog.LogManager.Shutdown();
        }
    }
}
