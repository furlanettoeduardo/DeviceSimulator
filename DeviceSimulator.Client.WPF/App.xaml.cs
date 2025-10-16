using System.Configuration;
using System.Data;
using System.Windows;
using DeviceSimulator.Client.WPF.Logging;

namespace DeviceSimulator.Client.WPF;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DevLogger.Initialize("client");
        // Enable gRPC over HTTP/2 without TLS (http)
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        DevLogger.Info("Application exiting");
        DevLogger.Dispose();
        base.OnExit(e);
    }
}

