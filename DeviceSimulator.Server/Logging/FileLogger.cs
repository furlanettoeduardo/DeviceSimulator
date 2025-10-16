using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace DeviceSimulator.Server.Logging;

internal sealed class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly LogLevel _minLevel;
    private IExternalScopeProvider? _scopeProvider;
    private readonly TextWriter _writer;
    private readonly object _lock = new object();

    public FileLogger(string categoryName, LogLevel minLevel, TextWriter writer, IExternalScopeProvider? scopeProvider)
    {
        _categoryName = categoryName;
        _minLevel = minLevel;
        _writer = writer;
        _scopeProvider = scopeProvider;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return _scopeProvider?.Push(state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _minLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var message = formatter(state, exception);

        var sb = new StringBuilder();
        sb.Append('[').Append(timestamp).Append("] [").Append(logLevel).Append("] [").Append(_categoryName).Append("] ");
        sb.Append(message);
        if (exception != null)
        {
            sb.Append(" | ").Append(exception.GetType().Name).Append(": ").Append(exception.Message);
        }

        lock (_lock)
        {
            _writer.WriteLine(sb.ToString());
            _writer.Flush();
        }
    }
}