using System;
using System.Threading.Tasks;
using DeviceSimulator.Proto;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace DeviceSimulator.Server.Services;

public class DeviceServiceImpl : DeviceService.DeviceServiceBase
{
    private readonly ILogger<DeviceServiceImpl> _logger;
    private bool _connected;

    public DeviceServiceImpl(ILogger<DeviceServiceImpl> logger)
    {
        _logger = logger;
    }

    public override Task<UpdateResponse> UpdateAxis(AxisUpdateRequest request, ServerCallContext context)
    {
        _connected = request.Connected;
        var axisName = AxisToName(request.Axis);
        _logger.LogInformation("Eixo: {Axis} | Valor: {Value}% | Status: {Status}", axisName, request.Value, _connected ? "Conectado" : "Desconectado");
        return Task.FromResult(new UpdateResponse { Ok = true });
    }

    public override Task<UpdateResponse> UpdateStatus(StatusUpdateRequest request, ServerCallContext context)
    {
        _connected = request.Connected;
        _logger.LogInformation("Status do dispositivo: {Status}", _connected ? "Conectado" : "Desconectado");
        return Task.FromResult(new UpdateResponse { Ok = true });
    }

    public override Task<ServerInfo> GetServerInfo(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new ServerInfo { Version = "1.0.0" });
    }

    private static string AxisToName(Axis axis)
    {
        return axis switch
        {
            Axis.Clutch => "Embreagem",
            Axis.Brake => "Freio",
            Axis.Throttle => "Acelerador",
            _ => "Indefinido"
        };
    }
}