using System.Collections.Concurrent;
using System.Net;
using Backend.Monitor;
using log4net;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Extensions;

/// <summary>
/// This has the core logic that creates and maintains connections to the proxy.
/// </summary>
internal class TunnelConnectionListener : IConnectionListener
{
    private static readonly ILog log = LogManager.GetLogger(typeof(TunnelConnectionListener));
    private readonly SemaphoreSlim _connectionLock;
    private readonly ConcurrentDictionary<ConnectionContext, ConnectionContext> _connections = new();
    private readonly Dictionary<Uri, TrackLifetimeConnectionContext> _connectionsDictionary = new();
    private readonly TunnelOptions _options;
    private readonly CancellationTokenSource _closedCts = new();
    private TrackLifetimeConnectionContext _currentConnection;
    private readonly HttpMessageInvoker _httpMessageInvoker = new(new SocketsHttpHandler
    {
        EnableMultipleHttp2Connections = true,
        PooledConnectionLifetime = Timeout.InfiniteTimeSpan,
        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan
    });

    public TunnelConnectionListener(TunnelOptions options, EndPoint endpoint)
    {
        _options = options;
        _connectionLock = new(options.MaxConnectionCount);
        EndPoint = endpoint;

        if (endpoint is not UriEndPoint2)
        {
            throw new NotSupportedException($"UriEndPoint is required for {options.Transport} transport");
        }
        
       // Task.Run(() => RunPeriodicTaskAsync(TimeSpan.FromSeconds(15), KillConnections));
    }

    public EndPoint EndPoint { get; }

    private Uri Uri => ((UriEndPoint2)EndPoint).Uri!;

    public async ValueTask<ConnectionContext?> AcceptAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_closedCts.Token, cancellationToken).Token;

            // Kestrel will keep an active accept call open as long as the transport is active
            await _connectionLock.WaitAsync(cancellationToken);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                  
                    var connection = new TrackLifetimeConnectionContext(_options.Transport switch
                    {
                        TransportType.WebSockets => await WebSocketConnectionContext.ConnectAsync(Uri, cancellationToken),
                        TransportType.HTTP2 => await HttpClientConnectionContext.ConnectAsync(_httpMessageInvoker, Uri, cancellationToken),
                        _ => throw new NotSupportedException(),
                    });

                    // Track this connection lifetime
                    _connections.TryAdd(connection, connection);
                    ConnectionMonitor.Instance.AddConnection(connection);
                    log.DebugFormat("Added connection for {0} with connection id {1}",Uri,connection.ConnectionId);
                    //_connectionsDictionary.Add(Uri);
                    _ = Task.Run(async () =>
                    {
                        // When the connection is disposed, release it
                        //csm does this really wait until disposed? I dont know
                        await connection.ExecutionTask;

                        _connections.TryRemove(connection, out _);
                        await ConnectionMonitor.Instance.RemoveConnection(connection);
                        log.DebugFormat("Removing connection for {0} with connection id {1}", Uri, connection.ConnectionId);
                        // Allow more connections in
                        _connectionLock.Release();
                    },
                    cancellationToken);
                    //_currentConnection = connection;
                    return connection;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // TODO: More sophisticated backoff and retry
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }
    public async ValueTask DisposeAsync()
    {
        List<Task>? tasks = null;

        foreach (var (_, connection) in _connections)
        {
            tasks ??= new();

            tasks.Add(connection.DisposeAsync().AsTask());
        }

        if (tasks is null)
        {
            return;
        }

        await Task.WhenAll(tasks);
    }

    public ValueTask UnbindAsync(CancellationToken cancellationToken = default)
    {
        _closedCts.Cancel();

        foreach (var (_, connection) in _connections)
        {
            // REVIEW: Graceful?
            connection.Abort();
        }

        return ValueTask.CompletedTask;
    }

    async Task RunPeriodicTaskAsync(TimeSpan interval, Func<Task> action)
    {
        while (true)
        {
            var delayTask = Task.Delay(interval);
            await action();
            await delayTask;
        }
    }

    async Task KillConnections()
    {
        //await  UnbindAsync();
        if (!_connections.Any())
            return;

        await UnbindAsync();
        //foreach (var connectionContext in _connections)
        //{
        //    connectionContext.Value.Abort();
        //}
    }

    
}