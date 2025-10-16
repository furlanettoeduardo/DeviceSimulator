using System;
using System.IO;
using System.Text;

namespace DeviceSimulator.Client.WPF.Logging;

public static class DevLogger
{
    private static readonly object _lock = new object();
    private static StreamWriter? _writer;
    private static int _minLevelValue;
    private static string _component = "client";
    private static bool _initialized;

    public static void Initialize(string componentName = "client")
    {
        if (_initialized) return;
        _component = componentName;
        var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                  ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                  ?? "Development";

        var isProd = string.Equals(env, "Production", StringComparison.OrdinalIgnoreCase);
        _minLevelValue = isProd ? (int)DevLogLevel.Warning : (int)DevLogLevel.Debug;

        var dir = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, $"client-{DateTime.Now:yyyy-MM-dd_HH-mm}.log");
        _writer = new StreamWriter(new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.Read))
        {
            AutoFlush = true
        };
        _initialized = true;
        Info($"Logger initialized (env={env})");
    }

    public static void Dispose()
    {
        lock (_lock)
        {
            _writer?.Dispose();
            _writer = null;
            _initialized = false;
        }
    }

    public static void Debug(string message) => Write(DevLogLevel.Debug, message);
    public static void Info(string message) => Write(DevLogLevel.Information, message);
    public static void Warn(string message) => Write(DevLogLevel.Warning, message);
    public static void Error(string message, Exception? ex = null) => Write(DevLogLevel.Error, message, ex);

    private static void Write(DevLogLevel level, string message, Exception? ex = null)
    {
        if (!_initialized) Initialize(_component);
        if ((int)level < _minLevelValue) return;

        var ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var line = new StringBuilder()
            .Append('[').Append(ts).Append("] [").Append(level).Append("] [").Append(_component).Append("] ")
            .Append(message);
        if (ex != null)
        {
            line.Append(" | ").Append(ex.GetType().Name).Append(": ").Append(ex.Message);
        }

        lock (_lock)
        {
            try
            {
                _writer?.WriteLine(line.ToString());
                _writer?.Flush();
            }
            catch { /* ignore file I/O errors */ }
        }

        try
        {
            Console.WriteLine(line.ToString());
        }
        catch { /* ignore console errors */ }
    }

    private enum DevLogLevel
    {
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4
    }
}