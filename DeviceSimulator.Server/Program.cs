using DeviceSimulator.Server.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;
using DeviceSimulator.Server.Logging;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

var isDevelopment = builder.Environment.IsDevelopment();

builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = isDevelopment;
});

// Add gRPC Reflection for development
if (isDevelopment)
{
    builder.Services.AddGrpcReflection();
}

// Configure Kestrel for HTTP/2 without TLS on localhost:5000
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000, o => o.Protocols = HttpProtocols.Http2);
});

// Configure logging before building the app
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
var logFile = $"server-{DateTime.Now:yyyy-MM-dd_HH-mm}.log";
builder.Logging.AddFileLogger(logDir, logFile, isDevelopment ? LogLevel.Debug : LogLevel.Warning);

var app = builder.Build();

app.MapGrpcService<DeviceServiceImpl>();

// Map gRPC Reflection service for development
if (isDevelopment)
{
    app.MapGrpcReflectionService();
}

app.MapGet("/", () => "DeviceSimulator gRPC server. Use a gRPC client to communicate.");

app.Run();
