using System;
using Microsoft.Extensions.Logging;

namespace BadProject.Configuration
{
    public static class LoggingConfiguration
    {
        public static ILoggerFactory CreateLoggerFactory()
        {
            return LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddConsole()
                    .AddDebug();
            });
        }
    }
}