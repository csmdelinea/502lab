internal interface ICloseable
{
    bool IsClosed { get; }
    void Abort();
}

internal interface IWebSocketConnectionStream
{
    string ContextConnectionId { get; }
}