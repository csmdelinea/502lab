using System.Collections.Concurrent;
using log4net;
using System.Net.WebSockets;
using System.Runtime.InteropServices.JavaScript;
using Frontend.API;
using ToRefactor;
using Yarp.ReverseProxy.Forwarder;

public static class TunnelExensions
{
    private static readonly ILog log = LogManager.GetLogger(typeof(TunnelExensions));
    private static List<SocketModel> _sockets = new List<SocketModel>();
    private static ConcurrentDictionary<string,WebSocket> _webSockets = new ConcurrentDictionary<string,WebSocket>();
    public static IServiceCollection AddTunnelServices(this IServiceCollection services)
    {
        var tunnelFactory = new TunnelClientFactory();
        services.AddSingleton(tunnelFactory);
        services.AddSingleton<IForwarderHttpClientFactory>(tunnelFactory);

        return services;
    }


    public static ConcurrentDictionary<string, WebSocket> WebSockets => _webSockets;

    public static IEndpointConventionBuilder MapHttp2Tunnel(this IEndpointRouteBuilder routes, string path)
    {
        return routes.MapPost(path, static async (HttpContext context, string host, TunnelClientFactory tunnelFactory, IHostApplicationLifetime lifetime) =>
        {
            // HTTP/2 duplex stream
            if (context.Request.Protocol != HttpProtocol.Http2)
            {
                return Results.BadRequest();
            }

            var (requests, responses) = tunnelFactory.GetConnectionChannel(host);

            await requests.Reader.ReadAsync(context.RequestAborted);

            var stream = new DuplexHttpStream(context);

            using var reg = lifetime.ApplicationStopping.Register(() => stream.Abort());

            // Keep reusing this connection while, it's still open on the backend
            while (!context.RequestAborted.IsCancellationRequested)
            {
                // Make this connection available for requests
                await responses.Writer.WriteAsync(stream, context.RequestAborted);

                await stream.StreamCompleteTask;

                stream.Reset();
            }

            return EmptyResult.Instance;
        });
    }

    public static IEndpointConventionBuilder MapWebSocketTunnel(this IEndpointRouteBuilder routes, string path)
    {
        
        var conventionBuilder = routes.MapGet(path, static async (HttpContext context, string host, TunnelClientFactory tunnelFactory, IHostApplicationLifetime lifetime) =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                return Results.BadRequest();
            }

            var (requests, responses) = tunnelFactory.GetConnectionChannel(host);

            await requests.Reader.ReadAsync(context.RequestAborted);

            var ws = await context.WebSockets.AcceptWebSocketAsync();
           
            
            var now = DateTime.Now;
            //if (!_sockets.Any(n => n.Id == context.Connection.Id))
            //{
            //    var socketModel = new SocketModel
            //    {
            //        Id = context.Connection.Id,
            //        Created = now,
            //        CreatedDisplay = $"{now.Hour}:{now.Minute} {now.Second}:{now.Millisecond}",
            //        RemotePort = context.Connection.RemotePort,
            //        State = ws.State.ToString(),
            //    };
            //    _sockets.Add(socketModel);
            //    _webSockets.TryAdd(socketModel.Id, ws);
            //}
            ConnectionMonitorService.UpsertWebsocket(context.Connection.Id,ws);
            //ConnectionMonitorService.
            var stream = new WebSocketStream(context.Connection.Id,ws);
            
            // We should make this more graceful
            using var reg = lifetime.ApplicationStopping.Register(() => stream.Abort());
            //log.DebugFormat("Connection Tracking - Creating Connection for host {0} Connection Id {1}", host, context.Connection.Id);
            // Keep reusing this connection while, it's still open on the backend
            while (ws.State == WebSocketState.Open)
            {
                // Make this connection available for requests
                await responses.Writer.WriteAsync(stream, context.RequestAborted);

                await stream.StreamCompleteTask;

                stream.Reset();
            }


            return EmptyResult.Instance;
        });

        // Make this endpoint do websockets automagically as middleware for this specific route
        conventionBuilder.Add(e =>
        {
            var sub = routes.CreateApplicationBuilder();
            var websocketOptions = new WebSocketOptions();
            websocketOptions.KeepAliveInterval = TimeSpan.FromSeconds(1);
            
            sub.UseWebSockets(websocketOptions).Run(e.RequestDelegate!);
            e.RequestDelegate = sub.Build();
        });

        return conventionBuilder;
    }

    public static  void StartCleanup()
    {
        
        //Task.WaitAll([RunPeriodicTaskAsync(TimeSpan.FromSeconds(30), () => CleanupConnections())]);
    }
    private static async Task CleanupConnections()
    {
     //TODO: Need to log this stuff
        log.DebugFormat("Running cleanup task");
        foreach (var closedSocket in WebSockets.Where(n => n.Value.State != WebSocketState.Open))
        {
            log.DebugFormat("Sending Periodic Message on connection with state {0}", closedSocket.Value.CloseStatusDescription);
            var model = _sockets.SingleOrDefault(n => n.Id == closedSocket.Key);
            if(model != null)
                _sockets.Remove(model);

            log.DebugFormat("Cleaning up wegbsocket {0} in state {1}",closedSocket.Key, closedSocket.Value.State);
           // sockets.Remove(closedSocket.Key);
            closedSocket.Value.Dispose();
            //await WebSocketConnectionManager.RemoveSocket(closedSocket.Key);
        }
        //var bytes = Encoding.UTF8.GetBytes("Hello");
        //_logger.LogDebug("Sending Periodic Message on connection with state {0}", _clientWebSocket.State);
        //await _clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, token);
    }

    static async Task RunPeriodicTaskAsync(TimeSpan interval, Func<Task> action)
    {
        while (true)
        {
            var delayTask = Task.Delay(interval);
            await delayTask;
            await action();
        }
    }

    private static async Task SocketSnapshot()
    {
        foreach (var webSocket in WebSockets)
        {
            //var webServerSocket = webSocket as WebServerSocket;
            //var logObject = new {State=webSocket.Value.State, webSocket.Value.}
        }
    }
    // This is for .NET 6, .NET 7 has Results.Empty
    internal sealed class EmptyResult : IResult
    {
        internal static readonly EmptyResult Instance = new();

        public Task ExecuteAsync(HttpContext httpContext)
        {
            return Task.CompletedTask;
        }
    }
}