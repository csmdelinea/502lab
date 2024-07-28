using Microsoft.AspNetCore.Server.Kestrel.Core;
using ToRefactor;

var builder = WebApplication.CreateBuilder(args);


builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MinRequestBodyDataRate = null;
});

builder.Services.AddReverseProxy()
       .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Logging.AddLog4Net();

builder.Services.AddTunnelServices();

builder.Services.AddControllers();

var app = builder.Build();

app.MapReverseProxy();

// Uncomment to support websocket connections
app.MapWebSocketTunnel("/connect-ws");

// Auth can be added to this endpoint and we can restrict it to certain points
// to avoid exteranl traffic hitting it
app.MapHttp2Tunnel("/connect-h2");

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
ConnectionMonitorService.StartMonitor();
//Task.Run(() => TunnelExensions.StartCleanup());

app.Run();
//Task.WaitAll([TunnelExensions.StartCleanup(),app.RunAsync()]);
