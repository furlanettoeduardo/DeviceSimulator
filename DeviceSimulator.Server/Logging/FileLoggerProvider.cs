using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace DeviceSimulator.Server.Logging;

internal sealed class FileLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly LogLevel _minLevel;
    private IExternalScopeProvider? _scopeProvider;
    private readonly StreamWriter _writer;

    public FileLoggerProvider(string logDirectory, string fileName, LogLevel minLevel)
    {
        Directory.CreateDirectory(logDirectory);
        var path = Path.Combine(logDirectory, fileName);
        var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
        _writer = new StreamWriter(stream) { AutoFlush = true };
        _minLevel = minLevel;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, _minLevel, _writer, _scopeProvider);
    }

    public void Dispose()
    {
        _writer.Dispose();
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }
}