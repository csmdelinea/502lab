using System.Net.WebSockets;
using Backend.Transport;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Connections.Client;

public class WebSocketConnectionContext : HttpConnection
{
    private readonly CancellationTokenSource _cts = new();
    private WebSocket? _underlyingWebSocket;
    private readonly string? _connectionId;
    private readonly Uri _connectionUri;
    private readonly string _connectionTrackingId;

    public WebSocketConnectionContext(HttpConnectionOptions options, Uri connectionUri) :
        base(options, null)
    {

        this._connectionUri = connectionUri;

        _connectionId = Guid.NewGuid().ToString();
     
    }

    public override CancellationToken ConnectionClosed
    {
        get => _cts.Token;
        set { }
    }


    public WebSocket? UnderlyingWebSocket=>_underlyingWebSocket;
    //public string ContextConnectionId { get; set; }
    public Uri ConnectionUri => _connectionUri;

    //csm we are skipping negotiations so set this as it will be null otherwise
    public override string? ConnectionId
    {
        get { return _connectionId; }
    }

    public override void Abort()
    {

        _cts.Cancel();
        _underlyingWebSocket?.Abort();
    }

    public override void Abort(ConnectionAbortedException abortReason)
    {
       
        _cts.Cancel();
        _underlyingWebSocket?.Abort();
    }

    public override ValueTask DisposeAsync()
    {
     
        // REVIEW: Why doesn't dispose just work?
        Abort();

        return base.DisposeAsync();
    }

    internal static async ValueTask<WebSocketConnectionContext> ConnectAsync(Uri uri, CancellationToken cancellationToken)
    {
        ClientWebSocket? underlyingWebSocket = null;
        var options = new HttpConnectionOptions
        {
            Url = uri,
            Transports = HttpTransportType.WebSockets,
            SkipNegotiation = true,
            WebSocketFactory = async (context, cancellationToken) =>
            {
                underlyingWebSocket = new ClientWebSocket();
                underlyingWebSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(25);
                await underlyingWebSocket.ConnectAsync(context.Uri, cancellationToken);
                
                return underlyingWebSocket;
            }
        };
        
        var connection = new WebSocketConnectionContext(options,uri);
        
        await connection.StartAsync(TransferFormat.Binary, cancellationToken);
        connection._underlyingWebSocket = underlyingWebSocket;
        return connection;
    }
}