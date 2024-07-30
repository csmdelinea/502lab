using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace ToRefactor
{
    public class ConnectionMonitorService
    {
        //private static ConnectionMonitorService _instance;

        private ConnectionMonitorService()
        {

        }
        private static List<SocketModel> _socketModels = new List<SocketModel>();
        private static Dictionary<string, WebSocket> _webSockets = new();

        public static void StartMonitor()
        {
            //Task.Run(() => RunPeriodicTaskAsync(TimeSpan.FromSeconds(10), () => PingWebSockets()));
        }
        public static void UpsertWebsocket(HttpContext context, WebSocket webSocket)
        {

            _webSockets[context.Connection.Id] = webSocket;
            //if(_webSockets.ContainsKey(key))
            //    _webSockets[key] = webSocket;
            //else
            //{
            //    _webSockets.Add(key,webSocket);
            //}

            UpsertSocketModel(context, webSocket);
        }

        private static void UpsertSocketModel(HttpContext context, WebSocket webSocket)
        {
            var key = context.Connection.Id;
            var model = _socketModels.SingleOrDefault(n => n.Id == key);

            if (model == null)
            {
                var dt = DateTime.Now;
                model = new SocketModel
                {
                    Created = DateTime.Now,
                    CreatedDisplay =
                        $"{dt.Hour}:{dt.Minute.ToString().PadLeft(2, '0')} {dt.Second.ToString().PadLeft(2, '0')}.{dt.Millisecond.ToString().PadLeft(4, '0')}",
                    RemotePort = context.Connection.RemotePort,
                    Id = key
                };
                _socketModels.Add(model);
            }

            model.State = webSocket.State.ToString();
        }

        public static List<SocketModel> GetSocketModels()
        {
            return _socketModels.OrderBy(n => n.Created).ToList();
        }

        public static void RefreshSockets()
        {
            foreach (var webSocket in _webSockets)
            {
                var socketModel = _socketModels.SingleOrDefault(n => n.Id == webSocket.Key);
                if (socketModel != null)
                {
                    socketModel.State = webSocket.Value.State.ToString();
                }
            }
        }

        static async Task SocketSnapshot()
        {
            foreach (var webSocket in _webSockets)
            {

                ConnectionTrackingLogger.LogWebSocket<ConnectionMonitorService>(webSocket.Value, webSocket.Key, "Connection Snapshot");
          
            }
        }
        static async Task PingWebSockets()
        {
            foreach (var webSocket in _webSockets)
            {

                ConnectionTrackingLogger.LogWebSocket<ConnectionMonitorService>(webSocket.Value, webSocket.Key, "Connection Monitor task");
                try
                {
                    await webSocket.Value.SendAsync(new ArraySegment<byte>(Array.Empty<byte>()),
                        WebSocketMessageType.Binary, true, CancellationToken.None);

                    //var b = new byte[4096];
                    //var receiveResult = await webSocket.Value.ReceiveAsync()
                }
                catch (Exception ex)
                {
                    var x = "y";
                }
            }
        }
    
    static async Task MonitorConnections()
    {
        ConnectionTrackingLogger.LogMessage<ConnectionMonitorService>("Monitor", $"Running Monitor on {_webSockets.Count} Connections");
        foreach (var webSocket in _webSockets)
        {
            try
            {
                _socketModels.Single(n => n.Id == webSocket.Key).State = webSocket.Value.State.ToString();
                if (webSocket.Value.State == WebSocketState.Open)
                {
                    var bytes = Array.Empty<byte>();
                    await webSocket.Value.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary,
                        false,
                        CancellationToken.None);
                }
                else if (webSocket.Value.State != WebSocketState.Aborted)
                {
                    webSocket.Value.Dispose();

                }
                _socketModels.Single(n => n.Id == webSocket.Key).State = webSocket.Value.State.ToString();
            }
            catch (WebSocketException ex)
            {
                ConnectionTrackingLogger.LogException<ConnectionMonitorService>(ex, webSocket.Key, "Exception Encountered running monitor");
                //await webSocket.Value.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Normal",
                //    CancellationToken.None);

                webSocket.Value.Dispose();
                _socketModels.Single(n => n.Id == webSocket.Key).State = webSocket.Value.State.ToString();

            }
            //if (webSocket.Value.State != WebSocketState.Open)
            //{
            //    ConnectionTrackingLogger.LogWebSocket<ConnectionMonitorService>(webSocket.Value, webSocket.Key, "Socket State Monitor");
            //    if(webSocket.Value.State != WebSocketState.Aborted)
            //        webSocket.Value.Dispose();
            //}

        }
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

}
}
