using log4net;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading.Tasks.Sources;

using System.Text;
using ToRefactor;

internal class WebSocketStream : Stream, IValueTaskSource<object?>, ICloseable, IWebSocketConnectionStream
{
    private readonly WebSocket _ws;
    private ManualResetValueTaskSourceCore<object?> _tcs = new() { RunContinuationsAsynchronously = true };
    private CancellationTokenSource _disposeTokenSource = new();
    private readonly object _sync = new();
    private readonly string _contextConnectionId;
    private static readonly ILog log = LogManager.GetLogger(typeof(WebSocketStream));
    public WebSocketStream(string contextConnectionId,WebSocket ws)
    {
        _ws = ws;
        _contextConnectionId = contextConnectionId;

    }

    internal ValueTask<object?> StreamCompleteTask => new(this, _tcs.Version);
    public bool IsClosed => _ws.State != WebSocketState.Open;

    public string ContextConnectionId => _contextConnectionId;

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override void Flush()
    {
        throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {

        return _ws.SendAsync(buffer, WebSocketMessageType.Binary, endOfMessage: false, cancellationToken);
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposeTokenSource.Token);

        var result = await _ws.ReceiveAsync(buffer, linkedCts.Token);

        //string message = Encoding.UTF8.GetString(buffer.ToArray(), 0, result.Count);
        //if (message == "ping")
        //{
        //        var x = "Y";
        //    return 0;
        //}
        ConnectionTrackingLogger.LogMessage<WebSocketStream>(ContextConnectionId,$"Received {result.MessageType}");
        if (result.MessageType == WebSocketMessageType.Close)
        {

            //log.DebugFormat("Connection Tracking - Received WebSocketMessageType.Close for ContextConnectionId {0}",ContextConnectionId);
            return 0;
        }

        return result.Count;
    }

    public void Abort()
    {
        log.DebugFormat("Connection Tracking - Aborting Socket for ContextConnectionId {0}", ContextConnectionId);
        // Debug.Assert(!Thread.CurrentThread.IsThreadPoolThread);
        _ws.Abort();

        // The shutdown path is currently synchronous but at least we're not blocking a threadpool thread
        // Attempt a graceful close
        //using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        //_ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", timeout.Token).GetAwaiter().GetResult();
        // Wait for closed to be sent back otherwise abort
        //if (_ws.State != WebSocketState.Closed)
        //{
        //    _ws.Abort();
        //}

        lock (_sync)
        {
            if (GetStatus(_tcs.Version) != ValueTaskSourceStatus.Pending)
            {
                return;
            }

            _tcs.SetResult(null);
        }
    }

    protected override void Dispose(bool disposing)
    {
        lock (_sync)
        {
            if (GetStatus(_tcs.Version) != ValueTaskSourceStatus.Pending)
            {
                log.DebugFormat("Connection Tracking - Token Completion Source is Pending for ContextConnectionId {0}", ContextConnectionId);
                return;
            }

            // Cancel the token to signal the read loop to stop
            _disposeTokenSource.Cancel();
            _disposeTokenSource.Dispose();

            // Create a fresh token source 
            _disposeTokenSource = new();

            // This might seem evil but we're using dispose to know if the stream
            // has been given discarded by http client. We trigger the continuation and take back ownership
            // of it here.
            log.DebugFormat("Connection Tracking - Setting Token Completion Source Value to Null for ContextConnectionId {0}", ContextConnectionId);
            _tcs.SetResult(null);
        }
    }

    public object? GetResult(short token)
    {
        return _tcs.GetResult(token);
    }

    public void Reset()
    {
        _tcs.Reset();
    }

    public ValueTaskSourceStatus GetStatus(short token)
    {
        return _tcs.GetStatus(token);
    }

    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
    {
        _tcs.OnCompleted(continuation, state, token, flags);
    }
}