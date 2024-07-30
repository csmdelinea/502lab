using Microsoft.AspNetCore.Http.Connections.Client;
using System.Net.WebSockets;
using System.Numerics;
using System.Text;
using ToRefactor;

namespace Backend.Monitor
{
    public class ConnectionMonitor
    {
        private static ConnectionMonitor _instance;
        private TrackLifetimeConnectionContext _initialConnection;
        private List<TrackLifetimeConnectionContext> _connections = new();
        private ConnectionMonitor()
        {
            // Initialization code here
        }

        public static ConnectionMonitor Instance
        {
            get
            {
                // If the instance hasn't been created yet, create it
                if (_instance == null)
                {
                    _instance = new ConnectionMonitor();
                    _instance.StartMonitor();
                }
                return _instance;
            }
        }


        public void AddConnection(TrackLifetimeConnectionContext context)
        {
            if (!_connections.Any())
                _initialConnection = context;
            _connections.Add(context);
        }

        public async Task CloseConnection(TrackLifetimeConnectionContext context)
        {
            var connectionContext = context.GetWebSocketConnectionContext();
            await connectionContext.UnderlyingWebSocket?.CloseOutputAsync(WebSocketCloseStatus.NormalClosure,
                "Normal", connectionContext.ConnectionClosed)!;
        }
        public void AbortConnection(TrackLifetimeConnectionContext context)
        {
            var connectionContext = context.GetWebSocketConnectionContext();
            connectionContext.Abort();
        }
        public async Task RemoveConnection(TrackLifetimeConnectionContext context)
        {
            _connections.Remove(context);
        }

        public IReadOnlyList<TrackLifetimeConnectionContext> GetConnections()
        {
            return _connections;
        }

        void StartMonitor()
        {
            //Task.Run(() => RunPeriodicTaskAsync(TimeSpan.FromSeconds(10), () => MonitorClose()));
            //Task.WaitAll([RunPeriodicTaskAsync(TimeSpan.FromSeconds(10), () => SendPings())]);
        }

        async Task MonitorClose()
        {
            var connections = _connections.Select(n => n.GetWebSocketConnectionContext()).ToList();
            var tasks = new List<Task>();
            foreach (var webSocketConnectionContext in connections.Where(n => n.ConnectionId != _initialConnection.ConnectionId))
            {
                if (webSocketConnectionContext.UnderlyingWebSocket != null)
                {
                    if (webSocketConnectionContext.UnderlyingWebSocket.State != WebSocketState.Open)
                    {
                        await webSocketConnectionContext.UnderlyingWebSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure, "Normal", webSocketConnectionContext.ConnectionClosed);
                        await webSocketConnectionContext.DisposeAsync();
                        var x = "Y";
                    }
                }
                //if (webSocketConnectionContext.UnderlyingWebSocket != null && webSocketConnectionContext.UnderlyingWebSocket.State == WebSocketState.Open)
                //{
                //    var bytes = Encoding.UTF8.GetBytes("ping");
                //    ConnectionTrackingLogger.LogMessage<ConnectionMonitor>(webSocketConnectionContext.ConnectionId, $"Sending ping message");
                //    tasks.Add(webSocketConnectionContext.UnderlyingWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None));

                //}
            }

            if (!tasks.Any())
                return;
            await Task.WhenAll(tasks.ToArray());
        }
        async Task SendPings()
        {
            ConnectionTrackingLogger.LogMessage<ConnectionMonitor>("None", $"Sending pings");
            var connections = _connections.Select(n => n.GetWebSocketConnectionContext()).ToList();
            var tasks = new List<Task>();
            foreach (var webSocketConnectionContext in connections)
            {
                if (webSocketConnectionContext.UnderlyingWebSocket != null && webSocketConnectionContext.UnderlyingWebSocket.State == WebSocketState.Open)
                {
                    var bytes = Encoding.UTF8.GetBytes("ping");
                    ConnectionTrackingLogger.LogMessage<ConnectionMonitor>(webSocketConnectionContext.ConnectionId, $"Sending ping message");
                    tasks.Add(webSocketConnectionContext.UnderlyingWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None));

                }
            }

            if (!tasks.Any())
                return;
            await Task.WhenAll(tasks.ToArray());
        }

        async Task RunPeriodicTaskAsync(TimeSpan interval, Func<Task> action)
        {
            while (true)
            {
                var delayTask = Task.Delay(interval);
                await delayTask;
                await action();
            }
        }
    }

    public static class ConnectionsHelper
    {
        public static WebSocketConnectionContext GetWebSocketConnectionContext(this TrackLifetimeConnectionContext context)
        {
            var result =
                context.Features.Single(n => n.Value is WebSocketConnectionContext).Value as WebSocketConnectionContext;
            return result;
        }

    }


}
