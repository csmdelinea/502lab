using log4net;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;
using ToRefactor;
using Yarp.ReverseProxy.Forwarder;

/// <summary>
/// The factory that YARP will use the create outbound connections by host name.
/// </summary>
internal class TunnelClientFactory : ForwarderHttpClientFactory
{
    // TODO: These values should be populated by configuration so there's no need to remove
    // channels.
    private readonly ConcurrentDictionary<string, (Channel<int>, Channel<Stream>)> _clusterConnections = new();
    private readonly List<Stream> _streams = new List<Stream>();

    private static readonly ILog log = LogManager.GetLogger(typeof(TunnelClientFactory));
    public (Channel<int>, Channel<Stream>) GetConnectionChannel(string host)
    {
        return _clusterConnections.GetOrAdd(host, _ => (Channel.CreateUnbounded<int>(), Channel.CreateUnbounded<Stream>()));
    }

    protected override void ConfigureHandler(ForwarderHttpClientContext context, SocketsHttpHandler handler)
    {
        base.ConfigureHandler(context, handler);
        
        var previous = handler.ConnectCallback ?? DefaultConnectCallback;

        static async ValueTask<Stream> DefaultConnectCallback(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
        {
            throw new ArgumentNullException();
            //var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            //try
            //{
            //    await socket.ConnectAsync(context.DnsEndPoint, cancellationToken);
            //    return new NetworkStream(socket, ownsSocket: true);
            //}
            //catch
            //{
            //    socket.Dispose();
            //    throw;
            //}
        }
      //  Task.Run(() => RunPeriodicTaskAsync(TimeSpan.FromMinutes(3), ReportStreams));
        handler.ConnectCallback = async (context, cancellationToken) =>
        {
           

            if (_clusterConnections.TryGetValue(context.DnsEndPoint.Host, out var pair))
            {
  

                var (requests, responses) = pair;
                
                // Ask for a connection
                await requests.Writer.WriteAsync(0, cancellationToken);
                Stream stream;
                try
                {
                    while (true)
                    {
                        stream = await responses.Reader.ReadAsync(cancellationToken);
                        //var stream = responses.Reader.WaitToReadAsync(cancellationToken);
                        if (stream is ICloseable c && c.IsClosed)
                        {
                            // Ask for another connection
                            await requests.Writer.WriteAsync(0, cancellationToken);

                            continue;
                        }
                        //_streams.Add(stream);

                        return stream;
                    }
                }
                catch (Exception ex)
                {
                    throw;
                    //_clusterConnections.TryRemove(context.DnsEndPoint.Host,out pair);
                    ////handler.Dispose();
                    //var client = new HttpClient();
                    //await client.SendAsync(new HttpRequestMessage(HttpMethod.Get,
                    //    new Uri("https://localhost:7244/connect-ws?host=backend1.app")));
                    //await previous(context, cancellationToken);
                    //ConnectionTrackingLogger.LogException<TunnelClientFactory>(ex,"None",$"Exception during Connect Callback");

                }
            }
            return await previous(context, cancellationToken);
        };
    }

    //async Task ReportStreams()
    //{
    //    foreach (var s in _streams)
    //    {
    //        StringBuilder builder = new StringBuilder();
    //        foreach (var stream in _streams)
    //        {
    //            builder.AppendFormat("{0},{1}",stream.)
    //        }
    //        //s.Close();
    //        //await s.DisposeAsync();
    //    }
    //}


    async Task RunPeriodicTaskAsync(TimeSpan interval, Func<Task> action)
    {
        while (true)
        {
            var delayTask = Task.Delay(interval);
            await action();
            await delayTask;
        }
    }
}
