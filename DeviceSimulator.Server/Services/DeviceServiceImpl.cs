using System;
using System.Threading.Tasks;
using DeviceSimulator.Proto;
using Grpc.Core;

namespace DeviceSimulator.Server.Services;

public class DeviceServiceImpl : DeviceService.DeviceServiceBase
{
    private bool _connected;

    public override Task<UpdateResponse> UpdateAxis(AxisUpdateRequest request, ServerCallContext context)
    {
        _connected = request.Connected;
        var axisName = AxisToName(request.Axis);
        Console.WriteLine($"Eixo: {axisName} | Valor: {request.Value}% | Status: {( _connected ? "Conectado" : "Desconectado" )}");
        return Task.FromResult(new UpdateResponse { Ok = true });
    }

    public override Task<UpdateResponse> UpdateStatus(StatusUpdateRequest request, ServerCallContext context)
    {
        _connected = request.Connected;
        Console.WriteLine($"Status do dispositivo: {( _connected ? "Conectado" : "Desconectado" )}");
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