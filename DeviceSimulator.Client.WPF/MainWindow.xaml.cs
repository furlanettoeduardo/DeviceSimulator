using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DeviceSimulator.Proto;
using Grpc.Net.Client;
using DeviceSimulator.Client.WPF.Logging;
using System.Windows.Controls;

namespace DeviceSimulator.Client.WPF;

public partial class MainWindow : Window
{
    private DeviceService.DeviceServiceClient? _client;
    private bool _connected = false;
    private DispatcherTimer? _pingTimer;

    public MainWindow()
    {
        InitializeComponent();
        InitializeGrpc();
        StartPingTimer();
        this.Focusable = true;
        this.Focus();
    }

    private void InitializeGrpc()
    {
        try
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5000");
            _client = new DeviceService.DeviceServiceClient(channel);
        }
        catch (Exception ex)
        {
            DevLogger.Error("Failed to initialize gRPC client", ex);
            _client = null;
        }
    }

    private void StartPingTimer()
    {
        _pingTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _pingTimer.Tick += async (_, _) => await PingServerAsync();
        _pingTimer.Start();
    }

    private async Task PingServerAsync()
    {
        if (_client == null)
        {
            InitializeGrpc();
        }

        try
        {
            var info = await _client!.GetServerInfoAsync(new Empty());
            ServerStatusText.Text = "Servidor: Online";
            ServerStatusText.Foreground = System.Windows.Media.Brushes.Green;
            ServerVersionText.Text = $"Versão: {info.Version}";
            DevLogger.Debug("Server ping successful");
        }
        catch (Exception ex)
        {
            ServerStatusText.Text = "Servidor: Offline";
            ServerStatusText.Foreground = System.Windows.Media.Brushes.Red;
            ServerVersionText.Text = "Versão: -";
            DevLogger.Warn($"Server ping failed: {ex.Message}");
        }
    }

    private async Task SendAxisAsync(Axis axis, int value)
    {
        if (_client == null) return;
        try
        {
            await _client.UpdateAxisAsync(new AxisUpdateRequest
            {
                Axis = axis,
                Value = value,
                Connected = _connected
            });
        }
        catch (Exception ex)
        {
            DevLogger.Warn($"Failed to send axis update ({axis}, {value}%): {ex.Message}");
        }
    }

    private async Task SendStatusAsync()
    {
        if (_client == null) return;
        try
        {
            await _client.UpdateStatusAsync(new StatusUpdateRequest { Connected = _connected });
        }
        catch (Exception ex)
        {
            DevLogger.Warn($"Failed to send status update: {ex.Message}");
        }
    }
    private void UpdateBar(ProgressBar bar, TextBlock valueText, int value)
    {
        bar.Value = value;
        valueText.Text = $"{value}%";
    }

    private async void ConnectedToggle_Checked(object sender, RoutedEventArgs e)
    {
        _connected = true;
        ConnectedToggle.Content = "_Conectado";
        ConnectedToggle.Foreground = System.Windows.Media.Brushes.Green;
        DevLogger.Info("Device marked as connected");
        await SendStatusAsync();
    }

    private async void ConnectedToggle_Unchecked(object sender, RoutedEventArgs e)
    {
        _connected = false;
        ConnectedToggle.Content = "_Desconectado";
        ConnectedToggle.Foreground = System.Windows.Media.Brushes.Red;
        ConnectedToggle.ToolTip = null;
        DevLogger.Info("Device marked as disconnected");
        await SendStatusAsync();
    }

    private async void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        UpdateBar(ThrottleBar, ThrottleValueText, 100);
        await SendAxisAsync(Axis.Throttle, 100);
    }

    private async void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        UpdateBar(ThrottleBar, ThrottleValueText, 0);
        await SendAxisAsync(Axis.Throttle, 0);
    }

    private async void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.V)
        {
            UpdateBar(BrakeBar, BrakeValueText, 100);
            await SendAxisAsync(Axis.Brake, 100);
            e.Handled = true;
        }
        else if (e.Key == Key.C)
        {
            UpdateBar(ClutchBar, ClutchValueText, 100);
            await SendAxisAsync(Axis.Clutch, 100);
            e.Handled = true;
        }
    }

    private async void Window_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.V)
        {
            UpdateBar(BrakeBar, BrakeValueText, 0);
            await SendAxisAsync(Axis.Brake, 0);
            e.Handled = true;
        }
        else if (e.Key == Key.C)
        {
            UpdateBar(ClutchBar, ClutchValueText, 0);
            await SendAxisAsync(Axis.Clutch, 0);
            e.Handled = true;
        }
    }
}