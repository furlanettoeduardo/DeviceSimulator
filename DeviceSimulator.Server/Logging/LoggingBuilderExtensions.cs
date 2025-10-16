using System;
using Microsoft.Extensions.Logging;

namespace DeviceSimulator.Server.Logging;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string logDirectory, string fileName, LogLevel minLevel)
    {
        builder.AddProvider(new FileLoggerProvider(logDirectory, fileName, minLevel));
        return builder;
    }
}