
using System.Net;
using Frontend.Monitor;
using Yarp.ReverseProxy.Health;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
       .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddControllers();

builder.Logging.AddLog4Net();

builder.Services.AddTunnelServices();

builder.Services.AddSingleton<HealthMonitor>();

var app = builder.Build();

app.MapReverseProxy(proxyPipeline =>
{
   
});

// Uncomment to support websocket connections
app.MapWebSocketTunnel("/connect-ws");

// Auth can be added to this endpoint and we can restrict it to certain points
// to avoid exteranl traffic hitting it
app.MapHttp2Tunnel("/connect-h2");

//app.MapGet("/api/health", () =>
//{
//    return HttpStatusCode.OK;
//});

app.UseStaticFiles();
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    //endpoints.MapControllerRoute(
    //    name: "default",
    //    pattern: "{controller=Home}/{action=Index}/{id?}");

    endpoints.MapControllers(); // Add this line to map WebAPI controllers
});
app.Run();
