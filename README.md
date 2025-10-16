# Device Simulator

## Installation & Running

- Prerequisites
  - `dotnet SDK 8.0` installed.
  - Windows is required for the WPF client (`net8.0-windows`). The gRPC server runs cross‑platform.

- Build the solution
  - `dotnet build DeviceSimulator.sln`

- Start the gRPC server
  - `dotnet run --project DeviceSimulator.Server`
  - The server listens on `http://localhost:5000` using HTTP/2 without TLS.
  - A plain text check is available at `GET /` returning: `DeviceSimulator gRPC server. Use a gRPC client to communicate.`

- Start the WPF client
  - In another terminal: `dotnet run --project DeviceSimulator.Client.WPF`
  - The client connects to `http://localhost:5000` and updates the server status bar every second.

- Stopping
  - Client: close the window.
  - Server: press `Ctrl+C` in the terminal.

- Notes
  - gRPC over HTTP/2 without TLS is enabled in the client via `AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true)` (development only).
  - If you need another port, change it in `DeviceSimulator.Server/Program.cs` where Kestrel is configured (`options.ListenLocalhost(5000, ...)`).

## Features & How It Works

- Requested features from the initial brief
  - Manual connect control in the WPF client using a toggle button.
  - Simulation of three axes: `Throttle`, `Brake`, and `Clutch` with percentage values.
  - Keyboard and mouse mappings for quick interaction.
  - gRPC communication between client and server using `pedal.proto`.
  - Server status indicator in the client (Online/Offline + version).

- Client (WPF)
  - Toggle button controls the connection state (`Connected`/`Disconnected`) manually.
  - Sliders update axis values in real time: `Throttle`, `Brake`, `Clutch`.
  - Keyboard shortcuts:
    - `Space` sets `Brake` to 100 while pressed; released sets it to 0.
    - `C` sets `Clutch` to 100 while pressed; released sets it to 0.
  - Mouse:
    - Left button down sets `Throttle` to 100; up sets it to 0.
  - Status bar shows: `Server: Online`/`Offline` and `Version` from the server.
  - The client sends:
    - `UpdateStatus(connected)` when the toggle changes.
    - `UpdateAxis(axis, value, connected)` when a slider changes.
  - A `DispatcherTimer` pings the server (`GetServerInfo`) every second to refresh the status bar.

- Server (gRPC)
  - Service: `DeviceService` implements:
    - `UpdateAxis(AxisUpdateRequest)` returns `UpdateResponse`.
    - `UpdateStatus(StatusUpdateRequest)` returns `UpdateResponse`.
    - `GetServerInfo(Empty)` returns `ServerInfo` with a version string.
  - Kestrel is configured for HTTP/2 without TLS on `localhost:5000`.
  - Logs axis updates and connection status to the console.

- Protocol (Proto)
  - Enum `Axis`: `AXIS_UNSPECIFIED`, `CLUTCH`, `BRAKE`, `THROTTLE`.
  - Messages:
    - `AxisUpdateRequest { axis, value:int32, connected:bool }`
    - `StatusUpdateRequest { connected:bool }`
    - `UpdateResponse { ok:bool }`
    - `ServerInfo { version:string }`
    - `Empty {}`

- Design choices
  - The client uses manual connection control to keep behavior predictable and simple during development.
  - gRPC uses HTTP/2 without TLS for local development speed. Use TLS in production.
  - Server address is currently hardcoded in the client; it can be made configurable via settings in future iterations.

- Limitations
  - No TLS configured; not suitable for production as is.
  - No automated tests are included yet; the server is simple and logs actions for manual validation.
