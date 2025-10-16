using DeviceSimulator.Server.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

// Configure Kestrel for HTTP/2 without TLS on localhost:5000
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000, o => o.Protocols = HttpProtocols.Http2);
});

var app = builder.Build();

app.MapGrpcService<DeviceServiceImpl>();
app.MapGet("/", () => "DeviceSimulator gRPC server. Use a gRPC client to communicate.");

app.Run();
