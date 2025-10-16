using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DeviceSimulator.Proto;
using Grpc.Net.Client;

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
        catch (Exception)
        {
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
        }
        catch
        {
            ServerStatusText.Text = "Servidor: Offline";
            ServerStatusText.Foreground = System.Windows.Media.Brushes.Red;
            ServerVersionText.Text = "Versão: -";
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
        catch
        {
        }
    }

    private async Task SendStatusAsync()
    {
        if (_client == null) return;
        try
        {
            await _client.UpdateStatusAsync(new StatusUpdateRequest { Connected = _connected });
        }
        catch
        {
        }
    }
    private async void ThrottleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var value = (int)Math.Round(e.NewValue);
        ThrottleValueText.Text = $"{value}%";
        await SendAxisAsync(Axis.Throttle, value);
    }

    private async void BrakeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var value = (int)Math.Round(e.NewValue);
        BrakeValueText.Text = $"{value}%";
        await SendAxisAsync(Axis.Brake, value);
    }

    private async void ClutchSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var value = (int)Math.Round(e.NewValue);
        ClutchValueText.Text = $"{value}%";
        await SendAxisAsync(Axis.Clutch, value);
    }

    private async void ConnectedToggle_Checked(object sender, RoutedEventArgs e)
    {
        _connected = true;
        ConnectedToggle.Content = "Conectado";
        ConnectedToggle.Foreground = System.Windows.Media.Brushes.Green;
        await SendStatusAsync();
    }

    private async void ConnectedToggle_Unchecked(object sender, RoutedEventArgs e)
    {
        _connected = false;
        ConnectedToggle.Content = "Desconectado";
        ConnectedToggle.Foreground = System.Windows.Media.Brushes.Red;
        ConnectedToggle.ToolTip = null;
        await SendStatusAsync();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        ThrottleSlider.Value = 100;
    }

    private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        ThrottleSlider.Value = 0;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            BrakeSlider.Value = 100;
            e.Handled = true;
        }
        else if (e.Key == Key.C)
        {
            ClutchSlider.Value = 100;
            e.Handled = true;
        }
    }

    private void Window_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            BrakeSlider.Value = 0;
            e.Handled = true;
        }
        else if (e.Key == Key.C)
        {
            ClutchSlider.Value = 0;
            e.Handled = true;
        }
    }
}