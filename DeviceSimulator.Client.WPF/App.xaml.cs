using System.Configuration;
using System.Data;
using System.Windows;

namespace DeviceSimulator.Client.WPF;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // Enable gRPC over HTTP/2 without TLS (http)
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
    }
}

